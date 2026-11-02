using System;

namespace MlpProject
{
    /// <summary>
    /// Network-ready data: scaled feature matrices and target matrices for both splits,
    /// plus the fitted helpers needed to preprocess or decode new data later.
    /// </summary>
    public sealed class PreparedDataset
    {
        /// <summary>Gets or sets the scaled training features (one row per sample).</summary>
        public double[][] TrainFeatures { get; set; } = Array.Empty<double[]>();

        /// <summary>Gets or sets the training target vectors.</summary>
        public double[][] TrainTargets { get; set; } = Array.Empty<double[]>();

        /// <summary>Gets or sets the scaled testing features.</summary>
        public double[][] TestFeatures { get; set; } = Array.Empty<double[]>();

        /// <summary>Gets or sets the testing target vectors.</summary>
        public double[][] TestTargets { get; set; } = Array.Empty<double[]>();

        /// <summary>Gets or sets the scaler fitted on the training split.</summary>
        public MinMaxScaler Scaler { get; set; } = new MinMaxScaler();

        /// <summary>Gets or sets the label encoder fitted on the full dataset.</summary>
        public LabelEncoder LabelEncoder { get; set; } = new LabelEncoder();

        /// <summary>Gets the number of input features (input layer size).</summary>
        public int FeatureCount => Scaler.FeatureCount;
    }
}