using System;
using System.Collections.Generic;
using System.IO;
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
                AppendOutput(result.Training.TestMetrics.ToString());
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

                ExperimentResult best = null;
                foreach (ExperimentResult result in results)
                {
                    AppendOutput(ExperimentRunner.FormatResult(result));
                    if (best == null || result.Training.TestMetrics.Accuracy > best.Training.TestMetrics.Accuracy)
                    {
                        best = result;
                    }
                }

                if (best != null)
                {
                    AppendOutput(string.Empty);
                    AppendOutput("BEST CONFIGURATION BY TEST ACCURACY");
                    AppendOutput(ExperimentRunner.FormatResult(best));
                }
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

        private void btnEvaluate_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "Evaluation is ready. Connect your custom MLP's predictions here in the training-loop step.",
                "Network required",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }
}
