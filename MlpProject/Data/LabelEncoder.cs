using System;
using System.Collections.Generic;
using System.Linq;

namespace MlpProject
{
    /// <summary>
    /// Maps string class labels to numeric network targets and back.
    /// Labels are sorted alphabetically so the mapping is deterministic (for WDBC: B = 0, M = 1).
    /// </summary>
    public sealed class LabelEncoder
    {
        private Dictionary<string, int> _labelToIndex = new Dictionary<string, int>();
        private List<string> _indexToLabel = new List<string>();

        /// <summary>Gets the number of distinct classes learned.</summary>
        public int ClassCount => _indexToLabel.Count;

        /// <summary>
        /// Learns the set of distinct labels.
        /// </summary>
        /// <param name="labels">Labels from the dataset.</param>
        public void Fit(IEnumerable<string> labels)
        {
            _indexToLabel = labels.Distinct().OrderBy(label => label, StringComparer.Ordinal).ToList();

            if (_indexToLabel.Count < 2)
            {
                throw new InvalidOperationException("At least two distinct classes are required.");
            }

            _labelToIndex = new Dictionary<string, int>();
            for (int classIndex = 0; classIndex < _indexToLabel.Count; classIndex++)
            {
                _labelToIndex[_indexToLabel[classIndex]] = classIndex;
            }
        }

        /// <summary>Returns the integer class index for a label.</summary>
        /// <param name="label">The string label.</param>
        public int Encode(string label)
        {
            int index;
            if (!_labelToIndex.TryGetValue(label, out index))
            {
                throw new KeyNotFoundException($"Unknown label '{label}'.");
            }

            return index;
        }

        /// <summary>Returns the original string label for a class index.</summary>
        /// <param name="classIndex">The integer class index.</param>
        public string Decode(int classIndex)
        {
            return _indexToLabel[classIndex];
        }

        /// <summary>
        /// Builds the target vector for the output layer: a single value (0 or 1) for binary
        /// problems, or a one-hot vector for multi-class problems.
        /// </summary>
        /// <param name="label">The string label.</param>
        public double[] ToTargetVector(string label)
        {
            int classIndex = Encode(label);

            if (ClassCount == 2)
            {
                return new[] { (double)classIndex };
            }

            var oneHotVector = new double[ClassCount];
            oneHotVector[classIndex] = 1.0;
            return oneHotVector;
        }
    }
}