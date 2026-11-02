using System;
using System.Collections.Generic;

namespace MlpProject
{
    /// <summary>
    /// Contains the result produced after training and evaluation.
    /// </summary>
    public sealed class TrainingResult
    {
        /// <summary>Gets the final average training loss.</summary>
        public double FinalTrainingLoss { get; internal set; }

        /// <summary>Gets the average training loss recorded after every epoch (index 0 = epoch 1).</summary>
        public IReadOnlyList<double> LossHistory { get; internal set; } = new double[0];

        /// <summary>Gets the metrics calculated from the training set (used to spot overfitting).</summary>
        public ClassificationMetrics TrainMetrics { get; internal set; }

        /// <summary>Gets the metrics calculated from the test set.</summary>
        public ClassificationMetrics TestMetrics { get; internal set; }
    }

    /// <summary>
    /// Coordinates training, progress logging, prediction, and evaluation for a custom MLP.
    /// </summary>
    public static class TrainingLoop
    {
        /// <summary>
        /// Trains a network and evaluates it on the training and test splits.
        /// </summary>
        /// <param name="data">Preprocessed training and test data.</param>
        /// <param name="trainSample">
        /// Delegate that performs one training step and returns the loss for that sample.
        /// Connect this to the training method on the custom MLP.
        /// </param>
        /// <param name="predict">
        /// Delegate that returns the network output for one feature vector.
        /// Connect this to the prediction method on the custom MLP.
        /// </param>
        /// <param name="epochCount">Number of complete passes through the training data.</param>
        /// <param name="logInterval">Number of epochs between progress messages.</param>
        /// <param name="log">Optional progress logger; defaults to <see cref="Console.WriteLine(string)"/>.</param>
        /// <returns>The loss history, final loss, and train/test metrics.</returns>
        public static TrainingResult Run(
            PreparedDataset data,
            Func<double[], double[], double> trainSample,
            Func<double[], double[]> predict,
            int epochCount = 1000,
            int logInterval = 100,
            Action<string> log = null)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (trainSample == null) throw new ArgumentNullException(nameof(trainSample));
            if (predict == null) throw new ArgumentNullException(nameof(predict));
            if (data.TrainFeatures == null || data.TrainTargets == null || data.TrainFeatures.Length == 0)
            {
                throw new ArgumentException("The training split must contain at least one sample.", nameof(data));
            }

            if (data.TrainFeatures.Length != data.TrainTargets.Length)
            {
                throw new ArgumentException("Training features and targets must have the same length.", nameof(data));
            }

            if (data.TestFeatures == null || data.TestTargets == null || data.TestFeatures.Length == 0)
            {
                throw new ArgumentException("The test split must contain at least one sample.", nameof(data));
            }

            if (data.TestFeatures.Length != data.TestTargets.Length)
            {
                throw new ArgumentException("Test features and targets must have the same length.", nameof(data));
            }

            if (epochCount <= 0) throw new ArgumentOutOfRangeException(nameof(epochCount));
            if (logInterval <= 0) throw new ArgumentOutOfRangeException(nameof(logInterval));

            Action<string> writeLog = log ?? Console.WriteLine;
            double finalLoss = 0.0;
            var lossHistory = new List<double>(epochCount);

            for (int epoch = 1; epoch <= epochCount; epoch++)
            {
                double totalLoss = 0.0;

                for (int sampleIndex = 0; sampleIndex < data.TrainFeatures.Length; sampleIndex++)
                {
                    double loss = trainSample(
                        data.TrainFeatures[sampleIndex],
                        data.TrainTargets[sampleIndex]);

                    if (double.IsNaN(loss) || double.IsInfinity(loss))
                    {
                        throw new InvalidOperationException(
                            $"Training produced an invalid loss at epoch {epoch}, sample {sampleIndex}.");
                    }

                    totalLoss += loss;
                }

                finalLoss = totalLoss / data.TrainFeatures.Length;
                lossHistory.Add(finalLoss);

                if (epoch % logInterval == 0 || epoch == 1 || epoch == epochCount)
                {
                    writeLog($"Epoch {epoch}/{epochCount} - Loss: {finalLoss:F6}");
                }
            }

            return new TrainingResult
            {
                FinalTrainingLoss = finalLoss,
                LossHistory = lossHistory,
                TrainMetrics = Evaluator.Evaluate(data.TrainTargets, PredictAll(data.TrainFeatures, predict)),
                TestMetrics = Evaluator.Evaluate(data.TestTargets, PredictAll(data.TestFeatures, predict))
            };
        }

        /// <summary>Runs the network on every feature vector and collects the outputs in order.</summary>
        private static List<double[]> PredictAll(double[][] features, Func<double[], double[]> predict)
        {
            var outputs = new List<double[]>(features.Length);
            for (int sampleIndex = 0; sampleIndex < features.Length; sampleIndex++)
            {
                outputs.Add(predict(features[sampleIndex]));
            }

            return outputs;
        }
    }
}
