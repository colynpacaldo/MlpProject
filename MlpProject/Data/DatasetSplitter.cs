using System;
using System.Collections.Generic;
using System.Linq;

namespace MlpProject
{
    /// <summary>
    /// Splits records into training and testing subsets.
    /// </summary>
    public static class DatasetSplitter
    {
        /// <summary>
        /// Performs a seeded, stratified split so each class keeps (approximately) the same
        /// proportion in both subsets. With a fixed seed the split is fully reproducible.
        /// </summary>
        /// <param name="records">All records to split.</param>
        /// <param name="trainFraction">Fraction assigned to training, strictly between 0 and 1 (default 0.8).</param>
        /// <param name="randomSeed">Seed for the shuffling random number generator.</param>
        /// <returns>A tuple containing the training and testing records.</returns>
        public static (List<DataRecord> Train, List<DataRecord> Test) StratifiedSplit(
            IReadOnlyList<DataRecord> records,
            double trainFraction = 0.8,
            int randomSeed = 42)
        {
            if (trainFraction <= 0.0 || trainFraction >= 1.0)
            {
                throw new ArgumentOutOfRangeException(nameof(trainFraction), "Must be strictly between 0 and 1.");
            }

            var randomGenerator = new Random(randomSeed);
            var trainRecords = new List<DataRecord>();
            var testRecords = new List<DataRecord>();

            foreach (IGrouping<string, DataRecord> classGroup in records.GroupBy(r => r.Label).OrderBy(g => g.Key, StringComparer.Ordinal))
            {
                List<DataRecord> shuffledClassRecords = classGroup.ToList();
                Shuffle(shuffledClassRecords, randomGenerator);

                int classTrainCount = (int)Math.Round(shuffledClassRecords.Count * trainFraction, MidpointRounding.AwayFromZero);
                if (shuffledClassRecords.Count >= 2)
                {
                    // Guarantee every class appears in both subsets.
                    classTrainCount = Math.Max(1, Math.Min(classTrainCount, shuffledClassRecords.Count - 1));
                }

                trainRecords.AddRange(shuffledClassRecords.Take(classTrainCount));
                testRecords.AddRange(shuffledClassRecords.Skip(classTrainCount));
            }

            // Mix the classes so they are not stored in blocks.
            Shuffle(trainRecords, randomGenerator);
            Shuffle(testRecords, randomGenerator);

            return (trainRecords, testRecords);
        }

        /// <summary>
        /// In-place Fisher-Yates shuffle.
        /// </summary>
        private static void Shuffle<T>(IList<T> items, Random randomGenerator)
        {
            for (int currentIndex = items.Count - 1; currentIndex > 0; currentIndex--)
            {
                int swapIndex = randomGenerator.Next(currentIndex + 1);
                T temporaryItem = items[currentIndex];
                items[currentIndex] = items[swapIndex];
                items[swapIndex] = temporaryItem;
            }
        }
    }
}