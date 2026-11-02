using System.Collections.Generic;
using System.Linq;

namespace MlpProject
{
    /// <summary>
    /// Orchestrates the full preprocessing flow: parse, split, fit scaler on training data, scale, encode.
    /// </summary>
    public static class DataPreprocessor
    {
        /// <summary>
        /// Loads a CSV and produces a leakage-free, scaled train/test dataset.
        /// </summary>
        /// <param name="filePath">Path to the CSV file.</param>
        /// <param name="trainFraction">Fraction of data used for training (default 0.8).</param>
        /// <param name="randomSeed">Seed for the reproducible split.</param>
        /// <param name="hasHeaderRow">Whether the CSV has a header row.</param>
        /// <param name="labelColumnIndex">Zero-based label column index (WDBC: 1).</param>
        /// <param name="idColumnIndex">Zero-based ID column index to discard, or -1 (WDBC: 0).</param>
        /// <returns>A <see cref="PreparedDataset"/> ready to feed to the network.</returns>
        public static PreparedDataset Prepare(
            string filePath,
            double trainFraction = 0.8,
            int randomSeed = 42,
            bool hasHeaderRow = true,
            int labelColumnIndex = 1,
            int idColumnIndex = 0)
        {
            List<DataRecord> allRecords = DataParser.Parse(filePath, hasHeaderRow, labelColumnIndex, idColumnIndex);

            var labelEncoder = new LabelEncoder();
            labelEncoder.Fit(allRecords.Select(record => record.Label));

            var split = DatasetSplitter.StratifiedSplit(allRecords, trainFraction, randomSeed);
            List<DataRecord> trainRecords = split.Train;
            List<DataRecord> testRecords = split.Test;

            // Fit on training data ONLY to prevent data leakage, then apply to both splits.
            var scaler = new MinMaxScaler();
            scaler.Fit(trainRecords.Select(record => record.Features).ToList());

            return new PreparedDataset
            {
                TrainFeatures = scaler.TransformAll(trainRecords.Select(record => record.Features).ToList()),
                TrainTargets = trainRecords.Select(record => labelEncoder.ToTargetVector(record.Label)).ToArray(),
                TestFeatures = scaler.TransformAll(testRecords.Select(record => record.Features).ToList()),
                TestTargets = testRecords.Select(record => labelEncoder.ToTargetVector(record.Label)).ToArray(),
                Scaler = scaler,
                LabelEncoder = labelEncoder
            };
        }
    }
}