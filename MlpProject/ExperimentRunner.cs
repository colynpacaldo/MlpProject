using System;
using System.Collections.Generic;
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

    /// <summary>Contains one configuration and its measured test-set result.</summary>
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
        /// <summary>
        /// Prepares the included CSV and runs one MLP configuration.
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

            var training = TrainingLoop.Run(
                data,
                network.TrainSample,
                network.Predict,
                configuration.EpochCount,
                Math.Max(1, configuration.EpochCount / 10),
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
                Action<string> experimentLog = message =>
                {
                    if (log != null) log($"[{configuration}] {message}");
                };

                results.Add(Run(data, configuration, experimentLog));
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

        /// <summary>Formats a result for the UI and README experiment notes.</summary>
        public static string FormatResult(ExperimentResult result)
        {
            ClassificationMetrics metrics = result.Training.TestMetrics;
            return $"{result.Configuration} | Loss={result.Training.FinalTrainingLoss:F6} | " +
                   $"Accuracy={metrics.Accuracy:P2} | Precision={metrics.Precision:P2} | Recall={metrics.Recall:P2}";
        }
    }
}
