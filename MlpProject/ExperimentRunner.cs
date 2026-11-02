using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MlpProject
{
    /// <summary>Defines one reproducible training configuration.</summary>
    public sealed class TrainingConfiguration
    {
        /// <summary>Gets or sets the gradient-descent learning rate.</summary>
        public double LearningRate { get; set; }

        /// <summary>Gets or sets the hidden-layer width.</summary>
        public int HiddenNodeCount { get; set; }

        /// <summary>Gets or sets the number of training epochs.</summary>
        public int EpochCount { get; set; }

        /// <summary>Gets or sets the random seed used by the network.</summary>
        public int RandomSeed { get; set; } = 42;

        /// <summary>Returns a concise configuration label.</summary>
        public override string ToString()
        {
            return $"LR={LearningRate:0.###}, Hidden={HiddenNodeCount}, Epochs={EpochCount}";
        }
    }

    /// <summary>Contains one configuration and its measured result.</summary>
    public sealed class ExperimentResult
    {
        /// <summary>Gets the configuration that produced this result.</summary>
        public TrainingConfiguration Configuration { get; internal set; }

        /// <summary>Gets the completed training result.</summary>
        public TrainingResult Training { get; internal set; }
    }

    /// <summary>Runs the custom MLP and repeatable hyperparameter experiments.</summary>
    public static class ExperimentRunner
    {
        /// <summary>Maximum number of epochs between console/log progress lines.</summary>
        private const int MaxLogInterval = 100;

        /// <summary>
        /// Trains and evaluates one MLP configuration on already prepared data.
        /// </summary>
        /// <param name="data">Prepared train/test data.</param>
        /// <param name="configuration">Network and training settings.</param>
        /// <param name="log">Optional progress callback.</param>
        public static ExperimentResult Run(
            PreparedDataset data,
            TrainingConfiguration configuration,
            Action<string> log = null)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            var network = new MlpNetwork(
                data.FeatureCount,
                configuration.HiddenNodeCount,
                data.TrainTargets[0].Length,
                configuration.LearningRate,
                configuration.RandomSeed);

            // Log the loss every 100 epochs (or fewer for short runs), as the activity requires.
            int logInterval = Math.Max(1, Math.Min(MaxLogInterval, configuration.EpochCount));

            var training = TrainingLoop.Run(
                data,
                network.TrainSample,
                network.Predict,
                configuration.EpochCount,
                logInterval,
                log);

            return new ExperimentResult
            {
                Configuration = configuration,
                Training = training
            };
        }

        /// <summary>
        /// Runs a fixed set of configurations against the same prepared split.
        /// </summary>
        /// <param name="data">Prepared train/test data reused for every experiment.</param>
        /// <param name="configurations">Configurations to compare.</param>
        /// <param name="log">Optional progress callback.</param>
        public static IList<ExperimentResult> RunTuning(
            PreparedDataset data,
            IEnumerable<TrainingConfiguration> configurations,
            Action<string> log = null)
        {
            if (configurations == null) throw new ArgumentNullException(nameof(configurations));

            var results = new List<ExperimentResult>();
            foreach (TrainingConfiguration configuration in configurations)
            {
                TrainingConfiguration current = configuration;
                Action<string> experimentLog = message =>
                {
                    if (log != null) log($"[{current}] {message}");
                };

                results.Add(Run(data, current, experimentLog));
            }

            return results;
        }

        /// <summary>
        /// Creates the configurations used by the UI tuning demonstration.
        /// </summary>
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

        /// <summary>
        /// Picks the best result: highest test accuracy, ties broken by higher recall
        /// (missing a malignant case is the costliest error), then by lower final loss.
        /// </summary>
        /// <returns>The best result, or null when the sequence is empty.</returns>
        public static ExperimentResult SelectBest(IEnumerable<ExperimentResult> results)
        {
            if (results == null) throw new ArgumentNullException(nameof(results));

            return results
                .OrderByDescending(r => r.Training.TestMetrics.Accuracy)
                .ThenByDescending(r => r.Training.TestMetrics.Recall)
                .ThenBy(r => r.Training.FinalTrainingLoss)
                .FirstOrDefault();
        }

        /// <summary>Formats a result for the UI and README experiment notes.</summary>
        public static string FormatResult(ExperimentResult result)
        {
            ClassificationMetrics test = result.Training.TestMetrics;
            ClassificationMetrics train = result.Training.TrainMetrics;
            return $"{result.Configuration} | Loss={result.Training.FinalTrainingLoss:F6} | " +
                   $"TrainAcc={train.Accuracy:P2} | TestAcc={test.Accuracy:P2} | " +
                   $"Precision={test.Precision:P2} | Recall={test.Recall:P2}";
        }

        /// <summary>
        /// Formats one README tuning-table row (the Observations column is left blank on purpose).
        /// </summary>
        public static string FormatMarkdownRow(ExperimentResult result)
        {
            TrainingConfiguration c = result.Configuration;
            double accuracyPercent = result.Training.TestMetrics.Accuracy * 100.0;
            return string.Format(
                CultureInfo.InvariantCulture,
                "| {0:0.000} | {1} | {2} | {3:F6} | {4:F2}% |  |",
                c.LearningRate,
                c.HiddenNodeCount,
                c.EpochCount,
                result.Training.FinalTrainingLoss,
                accuracyPercent);
        }

        /// <summary>
        /// Formats a binary confusion matrix (rows = actual, columns = predicted) as text.
        /// Class 0 = B (benign), class 1 = M (malignant).
        /// </summary>
        public static string FormatConfusionMatrix(ClassificationMetrics metrics)
        {
            if (metrics == null) throw new ArgumentNullException(nameof(metrics));
            if (!metrics.IsBinary) return string.Empty;

            ConfusionMatrix m = metrics.Matrix;
            string nl = Environment.NewLine;
            return "Confusion matrix (rows = actual, columns = predicted):" + nl +
                   "              Pred B  Pred M" + nl +
                   $"Actual B   {m.GetCount(0, 0),8}{m.GetCount(0, 1),8}" + nl +
                   $"Actual M   {m.GetCount(1, 0),8}{m.GetCount(1, 1),8}";
        }
    }
}
