using System;
using System.Collections.Generic;
using System.Linq;

namespace MlpProject
{
    /// <summary>
    /// Immutable set of performance metrics computed from a <see cref="ConfusionMatrix"/>.
    /// <para>
    /// Edge-case policy: whenever a metric is mathematically undefined (a zero denominator),
    /// it is reported as 0.0 rather than NaN. Precision of a class is undefined when the model
    /// never predicts that class (TP + FP = 0); recall is undefined when the class never occurs
    /// in the data (TP + FN = 0).
    /// </para>
    /// </summary>
    public sealed class ClassificationMetrics
    {
        /// <summary>The class index treated as "positive" in binary problems (for WDBC: M = 1).</summary>
        public const int BinaryPositiveClassIndex = 1;

        /// <summary>Gets the confusion matrix these metrics were derived from.</summary>
        public ConfusionMatrix Matrix { get; }

        /// <summary>Gets the number of evaluated samples.</summary>
        public int SampleCount => Matrix.TotalCount;

        /// <summary>Gets a value indicating whether this is a two-class problem.</summary>
        public bool IsBinary => Matrix.ClassCount == 2;

        /// <summary>Gets accuracy: correct predictions divided by all predictions.</summary>
        public double Accuracy { get; }

        /// <summary>Gets per-class precision: TP / (TP + FP).</summary>
        public IReadOnlyList<double> PerClassPrecision { get; }

        /// <summary>Gets per-class recall: TP / (TP + FN).</summary>
        public IReadOnlyList<double> PerClassRecall { get; }

        /// <summary>Gets the number of actual samples in each class.</summary>
        public IReadOnlyList<int> ClassSupport { get; }

        /// <summary>Gets the unweighted mean of per-class precision across all classes.</summary>
        public double MacroPrecision { get; }

        /// <summary>Gets the unweighted mean of per-class recall across all classes.</summary>
        public double MacroRecall { get; }

        /// <summary>
        /// Gets the headline precision: the positive-class precision for binary problems,
        /// otherwise the macro average.
        /// </summary>
        public double Precision => IsBinary ? PerClassPrecision[BinaryPositiveClassIndex] : MacroPrecision;

        /// <summary>
        /// Gets the headline recall: the positive-class recall for binary problems,
        /// otherwise the macro average.
        /// </summary>
        public double Recall => IsBinary ? PerClassRecall[BinaryPositiveClassIndex] : MacroRecall;

        /// <summary>
        /// Computes all metrics from a populated confusion matrix.
        /// </summary>
        /// <param name="matrix">The confusion matrix to summarize.</param>
        public ClassificationMetrics(ConfusionMatrix matrix)
        {
            Matrix = matrix ?? throw new ArgumentNullException(nameof(matrix));

            int classCount = matrix.ClassCount;
            var precisionValues = new double[classCount];
            var recallValues = new double[classCount];
            var supportValues = new int[classCount];

            for (int classIndex = 0; classIndex < classCount; classIndex++)
            {
                int truePositives = matrix.TruePositives(classIndex);
                int falsePositives = matrix.FalsePositives(classIndex);
                int falseNegatives = matrix.FalseNegatives(classIndex);

                precisionValues[classIndex] = MetricMath.SafeDivide(truePositives, truePositives + falsePositives);
                recallValues[classIndex] = MetricMath.SafeDivide(truePositives, truePositives + falseNegatives);
                supportValues[classIndex] = matrix.Support(classIndex);
            }

            PerClassPrecision = precisionValues;
            PerClassRecall = recallValues;
            ClassSupport = supportValues;
            MacroPrecision = precisionValues.Average();
            MacroRecall = recallValues.Average();
            Accuracy = MetricMath.SafeDivide(matrix.CorrectCount(), matrix.TotalCount);
        }

        /// <summary>
        /// Returns a short multi-line, human-readable summary.
        /// </summary>
        public override string ToString()
        {
            string scope = IsBinary ? "positive class" : "macro average";
            return $"Samples:   {SampleCount}{Environment.NewLine}" +
                   $"Accuracy:  {Accuracy:P2}{Environment.NewLine}" +
                   $"Precision: {Precision:P2} ({scope}){Environment.NewLine}" +
                   $"Recall:    {Recall:P2} ({scope})";
        }
    }
}