namespace MlpProject
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblLearningRate;
        private System.Windows.Forms.Label lblHiddenNodes;
        private System.Windows.Forms.Label lblEpochs;
        private System.Windows.Forms.NumericUpDown nudLearningRate;
        private System.Windows.Forms.NumericUpDown nudHiddenNodes;
        private System.Windows.Forms.NumericUpDown nudEpochs;
        private System.Windows.Forms.Button btnTrain;
        private System.Windows.Forms.Button btnTune;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.TextBox txtDatasetPath;
        private System.Windows.Forms.TextBox txtOutput;
        private System.Windows.Forms.Label lblDataset;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblDataset = new System.Windows.Forms.Label();
            this.txtDatasetPath = new System.Windows.Forms.TextBox();
            this.lblLearningRate = new System.Windows.Forms.Label();
            this.lblHiddenNodes = new System.Windows.Forms.Label();
            this.lblEpochs = new System.Windows.Forms.Label();
            this.nudLearningRate = new System.Windows.Forms.NumericUpDown();
            this.nudHiddenNodes = new System.Windows.Forms.NumericUpDown();
            this.nudEpochs = new System.Windows.Forms.NumericUpDown();
            this.btnTrain = new System.Windows.Forms.Button();
            this.btnTune = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.txtOutput = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.nudLearningRate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudHiddenNodes)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudEpochs)).BeginInit();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(20, 15);
            this.lblTitle.Text = "Custom MLP Classification Experiment";
            // 
            // lblDataset
            // 
            this.lblDataset.AutoSize = true;
            this.lblDataset.Location = new System.Drawing.Point(22, 58);
            this.lblDataset.Text = "Dataset:";
            // 
            // txtDatasetPath
            // 
            this.txtDatasetPath.Location = new System.Drawing.Point(82, 55);
            this.txtDatasetPath.ReadOnly = true;
            this.txtDatasetPath.Size = new System.Drawing.Size(660, 23);
            // 
            // labels
            // 
            this.lblLearningRate.AutoSize = true;
            this.lblLearningRate.Location = new System.Drawing.Point(22, 97);
            this.lblLearningRate.Text = "Learning rate:";
            this.lblHiddenNodes.AutoSize = true;
            this.lblHiddenNodes.Location = new System.Drawing.Point(185, 97);
            this.lblHiddenNodes.Text = "Hidden nodes:";
            this.lblEpochs.AutoSize = true;
            this.lblEpochs.Location = new System.Drawing.Point(348, 97);
            this.lblEpochs.Text = "Epochs:";
            // 
            // numeric controls
            // 
            this.nudLearningRate.DecimalPlaces = 3;
            this.nudLearningRate.Increment = 0.001M;
            this.nudLearningRate.Location = new System.Drawing.Point(105, 94);
            this.nudLearningRate.Maximum = 1M;
            this.nudLearningRate.Minimum = 0.001M;
            this.nudLearningRate.Size = new System.Drawing.Size(65, 23);
            this.nudLearningRate.Value = 0.01M;
            this.nudHiddenNodes.Location = new System.Drawing.Point(268, 94);
            this.nudHiddenNodes.Maximum = 200M;
            this.nudHiddenNodes.Minimum = 1M;
            this.nudHiddenNodes.Size = new System.Drawing.Size(65, 23);
            this.nudHiddenNodes.Value = 10M;
            this.nudEpochs.Increment = 100M;
            this.nudEpochs.Location = new System.Drawing.Point(397, 94);
            this.nudEpochs.Maximum = 10000M;
            this.nudEpochs.Minimum = 1M;
            this.nudEpochs.Size = new System.Drawing.Size(75, 23);
            this.nudEpochs.Value = 1000M;
            // 
            // buttons
            // 
            this.btnTrain.Location = new System.Drawing.Point(500, 92);
            this.btnTrain.Size = new System.Drawing.Size(115, 28);
            this.btnTrain.Text = "Train model";
            this.btnTrain.UseVisualStyleBackColor = true;
            this.btnTrain.Click += new System.EventHandler(this.btnTrain_Click);
            this.btnTune.Location = new System.Drawing.Point(625, 92);
            this.btnTune.Size = new System.Drawing.Size(117, 28);
            this.btnTune.Text = "Run tuning";
            this.btnTune.UseVisualStyleBackColor = true;
            this.btnTune.Click += new System.EventHandler(this.btnTune_Click);
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(22, 132);
            this.progressBar.Size = new System.Drawing.Size(720, 12);
            // 
            // txtOutput
            // 
            this.txtOutput.BackColor = System.Drawing.Color.White;
            this.txtOutput.Font = new System.Drawing.Font("Consolas", 9F);
            this.txtOutput.Location = new System.Drawing.Point(22, 160);
            this.txtOutput.Multiline = true;
            this.txtOutput.ReadOnly = true;
            this.txtOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtOutput.Size = new System.Drawing.Size(720, 250);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(770, 435);
            this.Controls.Add(this.txtOutput);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.btnTune);
            this.Controls.Add(this.btnTrain);
            this.Controls.Add(this.nudEpochs);
            this.Controls.Add(this.nudHiddenNodes);
            this.Controls.Add(this.nudLearningRate);
            this.Controls.Add(this.lblEpochs);
            this.Controls.Add(this.lblHiddenNodes);
            this.Controls.Add(this.lblLearningRate);
            this.Controls.Add(this.txtDatasetPath);
            this.Controls.Add(this.lblDataset);
            this.Controls.Add(this.lblTitle);
            this.MinimumSize = new System.Drawing.Size(786, 474);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MLP Classification Experiment";
            ((System.ComponentModel.ISupportInitialize)(this.nudLearningRate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudHiddenNodes)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudEpochs)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}

