using System;

namespace MlpProject
{
    /// <summary>
    /// Represents a single labelled sample: a raw feature vector and its class label.
    /// </summary>
    public sealed class DataRecord
    {
        /// <summary>Gets the numeric feature values of this sample.</summary>
        public double[] Features { get; }

        /// <summary>Gets the original class label (for example "M" or "B").</summary>
        public string Label { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataRecord"/> class.
        /// </summary>
        /// <param name="features">The numeric feature values.</param>
        /// <param name="label">The class label.</param>
        public DataRecord(double[] features, string label)
        {
            Features = features ?? throw new ArgumentNullException(nameof(features));
            Label = label ?? throw new ArgumentNullException(nameof(label));
        }
    }
}