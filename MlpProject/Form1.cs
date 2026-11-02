using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MlpProject
{
    public partial class Form1 : Form
    {
        private readonly string _datasetPath;

        public Form1()
        {
            InitializeComponent();
            _datasetPath = Path.Combine(Application.StartupPath, "data.csv");
            txtDatasetPath.Text = _datasetPath;
        }

        private async void btnTrain_Click(object sender, EventArgs e)
        {
            SetBusy(true);
            txtOutput.Clear();
            AppendOutput("Preparing the Wisconsin Diagnostic Breast Cancer dataset...");

            try
            {
                TrainingConfiguration configuration = ReadConfiguration();
                ExperimentResult result = await Task.Run(() =>
                {
                    PreparedDataset data = DataPreprocessor.Prepare(_datasetPath, 0.8, 42);
                    return ExperimentRunner.Run(data, configuration, AppendOutput);
                });

                AppendOutput(string.Empty);
                AppendOutput("TRAINING COMPLETE");
                AppendOutput(ExperimentRunner.FormatResult(result));
                AppendOutput(string.Empty);
                AppendOutput("Test-set metrics:");
                AppendOutput(result.Training.TestMetrics.ToString());
                AppendOutput(string.Empty);
                AppendOutput(ExperimentRunner.FormatConfusionMatrix(result.Training.TestMetrics));

                var results = new List<ExperimentResult> { result };
                ShowLossChart(results);
                SaveResults(results, "train");
            }
            catch (Exception exception)
            {
                ShowError(exception);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async void btnTune_Click(object sender, EventArgs e)
        {
            SetBusy(true);
            txtOutput.Clear();
            AppendOutput("Running fixed-seed hyperparameter experiments...");

            try
            {
                IList<ExperimentResult> results = await Task.Run(() =>
                {
                    PreparedDataset data = DataPreprocessor.Prepare(_datasetPath, 0.8, 42);
                    return ExperimentRunner.RunTuning(
                        data,
                        ExperimentRunner.CreateDefaultTuningConfigurations(),
                        AppendOutput);
                });

                AppendOutput(string.Empty);
                AppendOutput("TUNING SUMMARY (test split)");
                foreach (ExperimentResult result in results)
                {
                    AppendOutput(ExperimentRunner.FormatResult(result));
                }

                AppendOutput(string.Empty);
                AppendOutput("README TABLE ROWS (paste into the tuning table, then write the Observations):");
                foreach (ExperimentResult result in results)
                {
                    AppendOutput(ExperimentRunner.FormatMarkdownRow(result));
                }

                ExperimentResult best = ExperimentRunner.SelectBest(results);
                if (best != null)
                {
                    AppendOutput(string.Empty);
                    AppendOutput("BEST CONFIGURATION (highest test accuracy; ties -> higher recall, then lower loss)");
                    AppendOutput(ExperimentRunner.FormatResult(best));
                    AppendOutput(ExperimentRunner.FormatConfusionMatrix(best.Training.TestMetrics));
                    AppendOutput("Note: choosing by test accuracy slightly favors this split; confirm with TrainAcc vs TestAcc.");
                }

                ShowLossChart(results);
                SaveResults(results, "tuning");
            }
            catch (Exception exception)
            {
                ShowError(exception);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private void btnOpenResults_Click(object sender, EventArgs e)
        {
            try
            {
                string directory = ResultsExporter.ResolveOutputDirectory();
                Process.Start("explorer.exe", "\"" + directory + "\"");
            }
            catch (Exception exception)
            {
                ShowError(exception);
            }
        }

        private TrainingConfiguration ReadConfiguration()
        {
            return new TrainingConfiguration
            {
                LearningRate = (double)nudLearningRate.Value,
                HiddenNodeCount = (int)nudHiddenNodes.Value,
                EpochCount = (int)nudEpochs.Value,
                RandomSeed = 42
            };
        }

        /// <summary>Plots the per-epoch loss of every supplied experiment.</summary>
        private void ShowLossChart(IEnumerable<ExperimentResult> results)
        {
            lossChart.SetSeries(results.Select(
                r => new LossSeries(r.Configuration.ToString(), r.Training.LossHistory)));
        }

        /// <summary>Writes the CSV/text logs to the project's results folder and reports where they went.</summary>
        private void SaveResults(IList<ExperimentResult> results, string prefix)
        {
            try
            {
                string directory = ResultsExporter.Save(results, txtOutput.Text, prefix);
                AppendOutput(string.Empty);
                AppendOutput("Saved " + prefix + "_loss_history.csv, " + prefix + "_summary.csv and "
                             + prefix + "_console_log.txt to: " + directory);
            }
            catch (Exception exception)
            {
                AppendOutput("Could not save result files: " + exception.Message);
            }
        }

        private void AppendOutput(string message)
        {
            if (txtOutput.IsDisposed) return;

            if (txtOutput.InvokeRequired)
            {
                txtOutput.BeginInvoke(new Action<string>(AppendOutput), message);
                return;
            }

            txtOutput.AppendText(message + Environment.NewLine);
        }

        private void SetBusy(bool busy)
        {
            btnTrain.Enabled = !busy;
            btnTune.Enabled = !busy;
            progressBar.Style = busy ? ProgressBarStyle.Marquee : ProgressBarStyle.Blocks;
            if (!busy) progressBar.Value = progressBar.Maximum;
        }

        private void ShowError(Exception exception)
        {
            AppendOutput("ERROR: " + exception.Message);
            MessageBox.Show(this, exception.ToString(), "MLP Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
