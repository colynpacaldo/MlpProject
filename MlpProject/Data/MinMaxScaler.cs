using System;
using System.Collections.Generic;
using System.Linq;

namespace MlpProject
{
    /// <summary>
    /// Scales every feature independently to the closed interval [0, 1] using
    /// <c>(value - min) / (max - min)</c>. Parameters are learned from training data only.
    /// </summary>
    public sealed class MinMaxScaler
    {
        private double[] _featureMinimums;
        private double[] _featureMaximums;

        /// <summary>Gets a value indicating whether <see cref="Fit"/> has been called.</summary>
        public bool IsFitted => _featureMinimums != null;

        /// <summary>Gets the number of features the scaler was fitted on.</summary>
        public int FeatureCount => _featureMinimums == null ? 0 : _featureMinimums.Length;

        /// <summary>
        /// Learns the per-feature minimum and maximum from the supplied (training) vectors.
        /// </summary>
        /// <param name="featureVectors">Training feature vectors, all of equal length.</param>
        public void Fit(IReadOnlyList<double[]> featureVectors)
        {
            if (featureVectors == null || featureVectors.Count == 0)
            {
                throw new ArgumentException("At least one feature vector is required to fit the scaler.", nameof(featureVectors));
            }

            int featureCount = featureVectors[0].Length;
            double[] minimums = Enumerable.Repeat(double.PositiveInfinity, featureCount).ToArray();
            double[] maximums = Enumerable.Repeat(double.NegativeInfinity, featureCount).ToArray();

            foreach (double[] vector in featureVectors)
            {
                if (vector.Length != featureCount)
                {
                    throw new ArgumentException("All feature vectors must have the same length.", nameof(featureVectors));
                }

                for (int featureIndex = 0; featureIndex < featureCount; featureIndex++)
                {
                    minimums[featureIndex] = Math.Min(minimums[featureIndex], vector[featureIndex]);
                    maximums[featureIndex] = Math.Max(maximums[featureIndex], vector[featureIndex]);
                }
            }

            _featureMinimums = minimums;
            _featureMaximums = maximums;
        }

        /// <summary>
        /// Scales a single feature vector to [0, 1]. Values outside the fitted range are clamped,
        /// and constant features (max == min) map to 0.
        /// </summary>
        /// <param name="featureVector">The raw feature vector.</param>
        /// <returns>A new, scaled feature vector.</returns>
        public double[] Transform(double[] featureVector)
        {
            if (_featureMinimums == null || _featureMaximums == null)
            {
                throw new InvalidOperationException("The scaler must be fitted before it can transform data.");
            }

            if (featureVector.Length != _featureMinimums.Length)
            {
                throw new ArgumentException(
                    $"Expected {_featureMinimums.Length} features but received {featureVector.Length}.", nameof(featureVector));
            }

            var scaledVector = new double[featureVector.Length];
            for (int featureIndex = 0; featureIndex < featureVector.Length; featureIndex++)
            {
                double range = _featureMaximums[featureIndex] - _featureMinimums[featureIndex];
                if (range == 0.0)
                {
                    scaledVector[featureIndex] = 0.0;
                    continue;
                }

                double scaledValue = (featureVector[featureIndex] - _featureMinimums[featureIndex]) / range;
                scaledVector[featureIndex] = Math.Max(0.0, Math.Min(1.0, scaledValue));
            }

            return scaledVector;
        }

        /// <summary>
        /// Scales a collection of feature vectors to [0, 1].
        /// </summary>
        /// <param name="featureVectors">The raw feature vectors.</param>
        /// <returns>A jagged array of scaled vectors, in the same order.</returns>
        public double[][] TransformAll(IReadOnlyList<double[]> featureVectors)
        {
            return featureVectors.Select(vector => Transform(vector)).ToArray();
        }
    }
}