using System;
using System.Collections.Generic;

namespace MlpProject
{
    /// <summary>
    /// Contains the result produced after training and test-set evaluation.
    /// </summary>
    public sealed class TrainingResult
    {
        /// <summary>Gets the final average training loss.</summary>
        public double FinalTrainingLoss { get; internal set; }

        /// <summary>Gets the metrics calculated from the test set.</summary>
        public ClassificationMetrics TestMetrics { get; internal set; }
    }

    /// <summary>
    /// Coordinates training, progress logging, prediction, and evaluation for a custom MLP.
    /// </summary>
    public static class TrainingLoop
    {
        /// <summary>
        /// Trains a network and evaluates it on the test split.
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
        /// <returns>The final loss and test-set metrics.</returns>
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

                if (epoch % logInterval == 0 || epoch == 1 || epoch == epochCount)
                {
                    writeLog($"Epoch {epoch}/{epochCount} - Loss: {finalLoss:F6}");
                }
            }

            var outputs = new List<double[]>(data.TestFeatures.Length);
            for (int sampleIndex = 0; sampleIndex < data.TestFeatures.Length; sampleIndex++)
            {
                outputs.Add(predict(data.TestFeatures[sampleIndex]));
            }

            return new TrainingResult
            {
                FinalTrainingLoss = finalLoss,
                TestMetrics = Evaluator.Evaluate(data.TestTargets, outputs)
            };
        }
    }

    /// <summary>Defines one reproducible training configuration.</summary>
    public sealed class TrainingConfiguration
    {
        public double LearningRate { get; set; }
        public int HiddenNodeCount { get; set; }
        public int EpochCount { get; set; }
        public int RandomSeed { get; set; } = 42;

        public override string ToString()
        {
            return $"LR={LearningRate:0.###}, Hidden={HiddenNodeCount}, Epochs={EpochCount}";
        }
    }

    /// <summary>Contains one configuration and its measured test-set result.</summary>
    public sealed class ExperimentResult
    {
        public TrainingConfiguration Configuration { get; internal set; }
        public TrainingResult Training { get; internal set; }
    }

    /// <summary>Runs the custom MLP and repeatable hyperparameter experiments.</summary>
    public static class ExperimentRunner
    {
        public static ExperimentResult Run(PreparedDataset data, TrainingConfiguration configuration, Action<string> log = null)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            var network = new MlpNetwork(data.FeatureCount, configuration.HiddenNodeCount,
                data.TrainTargets[0].Length, configuration.LearningRate, configuration.RandomSeed);
            var training = TrainingLoop.Run(data, network.TrainSample, network.Predict,
                configuration.EpochCount, Math.Max(1, configuration.EpochCount / 10), log);

            return new ExperimentResult { Configuration = configuration, Training = training };
        }

        public static IList<ExperimentResult> RunTuning(
            PreparedDataset data, IEnumerable<TrainingConfiguration> configurations, Action<string> log = null)
        {
            if (configurations == null) throw new ArgumentNullException(nameof(configurations));

            var results = new List<ExperimentResult>();
            foreach (TrainingConfiguration configuration in configurations)
            {
                Action<string> experimentLog = message =>
                {
                    if (log != null) log($"[{configuration}] {message}");
                };
                results.Add(Run(data, configuration, experimentLog));
            }
            return results;
        }

        public static IList<TrainingConfiguration> CreateDefaultTuningConfigurations()
        {
            return new List<TrainingConfiguration>
            {
                new TrainingConfiguration { LearningRate = 0.001, HiddenNodeCount = 10, EpochCount = 500 },
                new TrainingConfiguration { LearningRate = 0.005, HiddenNodeCount = 10, EpochCount = 500 },
                new TrainingConfiguration { LearningRate = 0.010, HiddenNodeCount = 10, EpochCount = 500 },
                new TrainingConfiguration { LearningRate = 0.050, HiddenNodeCount = 10, EpochCount = 500 },
                new TrainingConfiguration { LearningRate = 0.010, HiddenNodeCount = 5, EpochCount = 500 },
                new TrainingConfiguration { LearningRate = 0.010, HiddenNodeCount = 20, EpochCount = 500 },
                new TrainingConfiguration { LearningRate = 0.010, HiddenNodeCount = 30, EpochCount = 500 }
            };
        }

        public static string FormatResult(ExperimentResult result)
        {
            ClassificationMetrics metrics = result.Training.TestMetrics;
            return $"{result.Configuration} | Loss={result.Training.FinalTrainingLoss:F6} | " +
                   $"Accuracy={metrics.Accuracy:P2} | Precision={metrics.Precision:P2} | Recall={metrics.Recall:P2}";
        }
    }
}
