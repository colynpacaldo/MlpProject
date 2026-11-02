using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace MlpProject
{
    /// <summary>
    /// Reads a delimited text file (CSV) into a list of <see cref="DataRecord"/> objects.
    /// </summary>
    public static class DataParser
    {
        /// <summary>
        /// Parses a CSV file. Every column that is neither the ID column nor the label column
        /// is treated as a numeric feature. Blank lines are skipped, and trailing empty fields
        /// (such as the stray empty column in some Kaggle exports) are ignored.
        /// </summary>
        /// <param name="filePath">Path to the CSV file.</param>
        /// <param name="hasHeaderRow">Whether the first non-empty line is a header to skip.</param>
        /// <param name="labelColumnIndex">Zero-based index of the class label column.</param>
        /// <param name="idColumnIndex">Zero-based index of an ID column to discard, or -1 if there is none.</param>
        /// <returns>The parsed records, in file order.</returns>
        /// <exception cref="FileNotFoundException">The file does not exist.</exception>
        /// <exception cref="FormatException">A row is malformed (missing value, non-numeric feature, or inconsistent column count).</exception>
        public static List<DataRecord> Parse(
            string filePath,
            bool hasHeaderRow = true,
            int labelColumnIndex = 1,
            int idColumnIndex = 0)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("The dataset file could not be found.", filePath);
            }

            var records = new List<DataRecord>();
            int expectedFeatureCount = -1;
            bool headerAlreadySkipped = !hasHeaderRow;
            int lineNumber = 0;

            foreach (string rawLine in File.ReadLines(filePath))
            {
                lineNumber++;

                if (string.IsNullOrWhiteSpace(rawLine))
                {
                    continue;
                }

                if (!headerAlreadySkipped)
                {
                    headerAlreadySkipped = true;
                    continue;
                }

                List<string> fields = SplitAndTrimFields(rawLine);

                int highestRequiredIndex = Math.Max(labelColumnIndex, idColumnIndex);
                if (fields.Count <= highestRequiredIndex)
                {
                    throw new FormatException($"Line {lineNumber}: expected more columns than the {fields.Count} found.");
                }

                string label = fields[labelColumnIndex];
                if (label.Length == 0)
                {
                    throw new FormatException($"Line {lineNumber}: the class label is empty.");
                }

                var featureValues = new List<double>(fields.Count);
                for (int columnIndex = 0; columnIndex < fields.Count; columnIndex++)
                {
                    if (columnIndex == labelColumnIndex || columnIndex == idColumnIndex)
                    {
                        continue;
                    }

                    double value;
                    if (!double.TryParse(fields[columnIndex], NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                    {
                        throw new FormatException(
                            $"Line {lineNumber}, column {columnIndex}: '{fields[columnIndex]}' is not a valid number.");
                    }

                    featureValues.Add(value);
                }

                if (expectedFeatureCount < 0)
                {
                    expectedFeatureCount = featureValues.Count;
                }

                if (featureValues.Count != expectedFeatureCount)
                {
                    throw new FormatException(
                        $"Line {lineNumber}: found {featureValues.Count} features but expected {expectedFeatureCount}.");
                }

                records.Add(new DataRecord(featureValues.ToArray(), label));
            }

            if (records.Count == 0)
            {
                throw new FormatException("The file contained no data rows.");
            }

            return records;
        }

        /// <summary>
        /// Splits a line on commas, trims whitespace and quotes, and removes trailing empty fields.
        /// </summary>
        private static List<string> SplitAndTrimFields(string line)
        {
            List<string> fields = line.Split(',')
                .Select(field => field.Trim().Trim('"').Trim())
                .ToList();

            while (fields.Count > 0 && fields[fields.Count - 1].Length == 0)
            {
                fields.RemoveAt(fields.Count - 1);
            }

            return fields;
        }
    }
}