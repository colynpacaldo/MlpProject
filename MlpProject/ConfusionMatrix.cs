using System;

namespace MlpProject
{
    /// <summary>
    /// Accumulates prediction outcomes in an N x N table. Rows are the actual classes and
    /// columns are the predicted classes, so <c>GetCount(a, p)</c> is the number of samples
    /// of actual class <c>a</c> that were predicted as class <c>p</c>.
    /// Works for binary problems (N = 2) and multi-class problems (N &gt; 2).
    /// </summary>
    public sealed class ConfusionMatrix
    {
        private readonly int[,] _counts;

        /// <summary>Gets the number of classes (rows and columns) in the matrix.</summary>
        public int ClassCount { get; }

        /// <summary>Gets the total number of samples recorded so far.</summary>
        public int TotalCount { get; private set; }

        /// <summary>
        /// Initializes an empty confusion matrix.
        /// </summary>
        /// <param name="classCount">Number of classes; must be at least 2.</param>
        public ConfusionMatrix(int classCount)
        {
            if (classCount < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(classCount), "At least two classes are required.");
            }

            ClassCount = classCount;
            _counts = new int[classCount, classCount];
        }

        /// <summary>
        /// Records one prediction.
        /// </summary>
        /// <param name="actualClass">Zero-based index of the true class.</param>
        /// <param name="predictedClass">Zero-based index of the predicted class.</param>
        public void Add(int actualClass, int predictedClass)
        {
            ValidateClassIndex(actualClass, nameof(actualClass));
            ValidateClassIndex(predictedClass, nameof(predictedClass));

            _counts[actualClass, predictedClass]++;
            TotalCount++;
        }

        /// <summary>
        /// Returns how many samples of <paramref name="actualClass"/> were predicted as <paramref name="predictedClass"/>.
        /// </summary>
        public int GetCount(int actualClass, int predictedClass)
        {
            ValidateClassIndex(actualClass, nameof(actualClass));
            ValidateClassIndex(predictedClass, nameof(predictedClass));
            return _counts[actualClass, predictedClass];
        }

        /// <summary>Samples of the class correctly predicted as that class.</summary>
        public int TruePositives(int classIndex)
        {
            ValidateClassIndex(classIndex, nameof(classIndex));
            return _counts[classIndex, classIndex];
        }

        /// <summary>Samples of other classes wrongly predicted as this class (column sum minus TP).</summary>
        public int FalsePositives(int classIndex)
        {
            ValidateClassIndex(classIndex, nameof(classIndex));

            int predictedAsClass = 0;
            for (int actualClass = 0; actualClass < ClassCount; actualClass++)
            {
                predictedAsClass += _counts[actualClass, classIndex];
            }

            return predictedAsClass - _counts[classIndex, classIndex];
        }

        /// <summary>Samples of this class wrongly predicted as another class (row sum minus TP).</summary>
        public int FalseNegatives(int classIndex)
        {
            return Support(classIndex) - TruePositives(classIndex);
        }

        /// <summary>Samples that are neither of this class nor predicted as this class.</summary>
        public int TrueNegatives(int classIndex)
        {
            return TotalCount - TruePositives(classIndex) - FalsePositives(classIndex) - FalseNegatives(classIndex);
        }

        /// <summary>The number of samples whose actual class is <paramref name="classIndex"/> (row sum).</summary>
        public int Support(int classIndex)
        {
            ValidateClassIndex(classIndex, nameof(classIndex));

            int actualCount = 0;
            for (int predictedClass = 0; predictedClass < ClassCount; predictedClass++)
            {
                actualCount += _counts[classIndex, predictedClass];
            }

            return actualCount;
        }

        /// <summary>The number of correctly classified samples (diagonal sum).</summary>
        public int CorrectCount()
        {
            int correctCount = 0;
            for (int classIndex = 0; classIndex < ClassCount; classIndex++)
            {
                correctCount += _counts[classIndex, classIndex];
            }

            return correctCount;
        }

        private void ValidateClassIndex(int classIndex, string parameterName)
        {
            if (classIndex < 0 || classIndex >= ClassCount)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName, $"Class index {classIndex} is outside the range 0 to {ClassCount - 1}.");
            }
        }
    }
}