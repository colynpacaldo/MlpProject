using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace MlpProject
{
    /// <summary>
    /// Saves training logs and experiment summaries as CSV/text files so the loss-per-epoch
    /// evidence required by the activity can be committed to the repository.
    /// </summary>
    public static class ResultsExporter
    {
        private const string ProjectFileName = "MlpProject.csproj";
        private const string ResultsFolderName = "results";

        /// <summary>
        /// Finds the <c>results</c> folder next to <c>MlpProject.csproj</c> (so it is committed
        /// with the source rather than hidden in bin/Debug) and creates it if needed.
        /// Falls back to a folder beside the executable when the project file is not found.
        /// </summary>
        public static string ResolveOutputDirectory()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            DirectoryInfo current = new DirectoryInfo(baseDirectory);

            while (current != null)
            {
                if (File.Exists(Path.Combine(current.FullName, ProjectFileName)))
                {
                    return EnsureDirectory(Path.Combine(current.FullName, ResultsFolderName));
                }

                current = current.Parent;
            }

            return EnsureDirectory(Path.Combine(baseDirectory, ResultsFolderName));
        }

        /// <summary>
        /// Writes <c>{prefix}_loss_history.csv</c>, <c>{prefix}_summary.csv</c> and
        /// <c>{prefix}_console_log.txt</c>.
        /// </summary>
        /// <param name="results">Completed experiments to export.</param>
        /// <param name="consoleText">The full text shown in the output window.</param>
        /// <param name="prefix">File-name prefix, for example "train" or "tuning".</param>
        /// <returns>The directory the files were written to.</returns>
        public static string Save(IList<ExperimentResult> results, string consoleText, string prefix)
        {
            if (results == null) throw new ArgumentNullException(nameof(results));
            if (string.IsNullOrWhiteSpace(prefix)) throw new ArgumentException("A file prefix is required.", nameof(prefix));

            string directory = ResolveOutputDirectory();
            WriteLossHistory(Path.Combine(directory, prefix + "_loss_history.csv"), results);
            WriteSummary(Path.Combine(directory, prefix + "_summary.csv"), results);
            File.WriteAllText(
                Path.Combine(directory, prefix + "_console_log.txt"),
                consoleText ?? string.Empty,
                new UTF8Encoding(false));
            return directory;
        }

        private static void WriteLossHistory(string path, IList<ExperimentResult> results)
        {
            var builder = new StringBuilder();
            builder.AppendLine("LearningRate,HiddenNodes,Epochs,Epoch,Loss");

            foreach (ExperimentResult result in results)
            {
                TrainingConfiguration c = result.Configuration;
                IReadOnlyList<double> history = result.Training.LossHistory;
                for (int i = 0; i < history.Count; i++)
                {
                    builder.AppendLine(string.Format(
                        CultureInfo.InvariantCulture,
                        "{0},{1},{2},{3},{4:F6}",
                        c.LearningRate, c.HiddenNodeCount, c.EpochCount, i + 1, history[i]));
                }
            }

            File.WriteAllText(path, builder.ToString(), new UTF8Encoding(false));
        }

        private static void WriteSummary(string path, IList<ExperimentResult> results)
        {
            var builder = new StringBuilder();
            builder.AppendLine("LearningRate,HiddenNodes,Epochs,FinalLoss,TrainAccuracy,TestAccuracy,TestPrecision,TestRecall,TP,FP,FN,TN");

            foreach (ExperimentResult result in results)
            {
                TrainingConfiguration c = result.Configuration;
                ClassificationMetrics test = result.Training.TestMetrics;
                ClassificationMetrics train = result.Training.TrainMetrics;
                int positive = ClassificationMetrics.BinaryPositiveClassIndex;

                builder.AppendLine(string.Format(
                    CultureInfo.InvariantCulture,
                    "{0},{1},{2},{3:F6},{4:F4},{5:F4},{6:F4},{7:F4},{8},{9},{10},{11}",
                    c.LearningRate, c.HiddenNodeCount, c.EpochCount,
                    result.Training.FinalTrainingLoss,
                    train.Accuracy, test.Accuracy, test.Precision, test.Recall,
                    test.Matrix.TruePositives(positive),
                    test.Matrix.FalsePositives(positive),
                    test.Matrix.FalseNegatives(positive),
                    test.Matrix.TrueNegatives(positive)));
            }

            File.WriteAllText(path, builder.ToString(), new UTF8Encoding(false));
        }

        private static string EnsureDirectory(string path)
        {
            Directory.CreateDirectory(path);
            return path;
        }
    }
}
