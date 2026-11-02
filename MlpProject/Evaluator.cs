using System;
using System.Collections.Generic;

namespace MlpProject
{
    /// <summary>
    /// Converts network targets and outputs into class predictions and evaluates them.
    /// Understands the two target formats produced by <see cref="LabelEncoder"/>:
    /// a single value (0 or 1) for binary problems, and a one-hot vector for multi-class problems.
    /// </summary>
    public static class Evaluator
    {
        /// <summary>
        /// Evaluates raw network outputs against the expected target vectors.
        /// </summary>
        /// <param name="expectedTargets">Target vectors (single 0/1 value, or one-hot).</param>
        /// <param name="networkOutputs">Network output vectors, same length and order as the targets.</param>
        /// <param name="binaryThreshold">For single-output networks, outputs at or above this value count as class 1 (default 0.5).</param>
        /// <returns>The computed <see cref="ClassificationMetrics"/>.</returns>
        public static ClassificationMetrics Evaluate(
            IReadOnlyList<double[]> expectedTargets,
            IReadOnlyList<double[]> networkOutputs,
            double binaryThreshold = 0.5)
        {
            if (expectedTargets == null) throw new ArgumentNullException(nameof(expectedTargets));
            if (networkOutputs == null) throw new ArgumentNullException(nameof(networkOutputs));

            if (expectedTargets.Count == 0)
            {
                throw new ArgumentException("At least one sample is required.", nameof(expectedTargets));
            }

            if (expectedTargets.Count != networkOutputs.Count)
            {
                throw new ArgumentException(
                    $"Got {expectedTargets.Count} targets but {networkOutputs.Count} outputs.", nameof(networkOutputs));
            }

            int vectorLength = expectedTargets[0].Length;
            int classCount = vectorLength == 1 ? 2 : vectorLength;
            var confusionMatrix = new ConfusionMatrix(classCount);

            for (int sampleIndex = 0; sampleIndex < expectedTargets.Count; sampleIndex++)
            {
                if (expectedTargets[sampleIndex].Length != vectorLength || networkOutputs[sampleIndex].Length != vectorLength)
                {
                    throw new ArgumentException(
                        $"Sample {sampleIndex}: target and output vectors must all have length {vectorLength}.");
                }

                int actualClass = VectorToClassIndex(expectedTargets[sampleIndex], binaryThreshold, sampleIndex);
                int predictedClass = VectorToClassIndex(networkOutputs[sampleIndex], binaryThreshold, sampleIndex);
                confusionMatrix.Add(actualClass, predictedClass);
            }

            return new ClassificationMetrics(confusionMatrix);
        }

        /// <summary>
        /// Evaluates predictions that are already class indices.
        /// </summary>
        /// <param name="actualClasses">True class index of each sample.</param>
        /// <param name="predictedClasses">Predicted class index of each sample.</param>
        /// <param name="classCount">Total number of classes.</param>
        /// <returns>The computed <see cref="ClassificationMetrics"/>.</returns>
        public static ClassificationMetrics Evaluate(
            IReadOnlyList<int> actualClasses,
            IReadOnlyList<int> predictedClasses,
            int classCount)
        {
            if (actualClasses == null) throw new ArgumentNullException(nameof(actualClasses));
            if (predictedClasses == null) throw new ArgumentNullException(nameof(predictedClasses));

            if (actualClasses.Count != predictedClasses.Count)
            {
                throw new ArgumentException("Actual and predicted lists must have the same length.", nameof(predictedClasses));
            }

            var confusionMatrix = new ConfusionMatrix(classCount);
            for (int sampleIndex = 0; sampleIndex < actualClasses.Count; sampleIndex++)
            {
                confusionMatrix.Add(actualClasses[sampleIndex], predictedClasses[sampleIndex]);
            }

            return new ClassificationMetrics(confusionMatrix);
        }

        /// <summary>
        /// Converts a target or output vector to a class index: thresholding for a single value,
        /// arg-max for a vector of several values.
        /// </summary>
        /// <param name="vector">The target or output vector.</param>
        /// <param name="binaryThreshold">Decision threshold for single-value vectors.</param>
        /// <param name="sampleIndex">Sample position, used only in error messages.</param>
        /// <returns>The zero-based class index.</returns>
        public static int VectorToClassIndex(double[] vector, double binaryThreshold = 0.5, int sampleIndex = -1)
        {
            if (vector == null || vector.Length == 0)
            {
                throw new ArgumentException("The vector must contain at least one value.", nameof(vector));
            }

            foreach (double value in vector)
            {
                if (double.IsNaN(value) || double.IsInfinity(value))
                {
                    throw new InvalidOperationException(
                        $"Sample {sampleIndex}: the vector contains NaN or Infinity. " +
                        "The network has probably diverged (try a lower learning rate or check input scaling).");
                }
            }

            if (vector.Length == 1)
            {
                return vector[0] >= binaryThreshold ? 1 : 0;
            }

            int bestClassIndex = 0;
            for (int classIndex = 1; classIndex < vector.Length; classIndex++)
            {
                if (vector[classIndex] > vector[bestClassIndex])
                {
                    bestClassIndex = classIndex;
                }
            }

            return bestClassIndex;
        }
    }
}