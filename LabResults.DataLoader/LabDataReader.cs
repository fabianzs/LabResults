using LabResults.Domain.Interfaces;
using LabResults.Domain.Models;
using System.Reflection;

namespace LabResults.DataLoader
{
    public class LabDataReader : IDataReader
    {
        private static readonly string[] ExpectedHeaders = new[]
        {
            "CLINIC_NO", "BARCODE", "PATIENT_ID", "PATIENT_NAME", "DOB", "GENDER",
            "COLLECTIONDATE", "COLLECTIONTIME", "TESTCODE", "TESTNAME", "RESULT",
            "UNIT", "REFRANGELOW", "REFRANGEHIGH", "NOTE", "NONSPECREFS"
        };

        public async Task<IEnumerable<LabData>?> ReadDataFromFileAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"The data file was not found at: {filePath}");
            }

            var lines = File.ReadLines(filePath);

            using (var enumerator = lines.GetEnumerator())
            {
                string headerLine = null;

                // 1. Skip comments and find the header line
                while (enumerator.MoveNext())
                {
                    var line = enumerator.Current;
                    if (line.StartsWith("#") || string.IsNullOrWhiteSpace(line))
                    {
                        continue; // Skip comments and empty lines
                    }
                    // The first non-comment line is the header
                    headerLine = line;
                    break;
                }

                if (headerLine == null)
                {
                    throw new InvalidOperationException("File contains no data or header line.");
                }

                // 2. Validate the Header
                var actualHeaders = headerLine.Split('|', StringSplitOptions.TrimEntries);

                if (actualHeaders.Length != ExpectedHeaders.Length)
                {
                    throw new InvalidOperationException($"Header validation failed: Expected {ExpectedHeaders.Length} columns but found {actualHeaders.Length}.");
                }

                var propertyMap = BuildPropertyMap(actualHeaders);

                for (int i = 0; i < ExpectedHeaders.Length; i++)
                {
                    if (!string.Equals(actualHeaders[i], ExpectedHeaders[i], StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException($"Header validation failed: Column at index {i} mismatch. Expected '{ExpectedHeaders[i]}' but found '{actualHeaders[i]}'.");
                    }
                }

                // Header is validated! Now process the rest of the lines as data.
                var dataList = new List<LabData>();

                // 3. Process the remaining lines (which are all data lines)
                while (enumerator.MoveNext())
                {
                    var line = enumerator.Current;
                    if (string.IsNullOrWhiteSpace(line)) continue; // Skip empty lines in data

                    var fields = line.Split('|');
                    if (fields.Length != actualHeaders.Length) continue;

                    // Create and populate the LabData object using the dynamic map
                    var rawData = MapFieldsToLabData(fields, propertyMap);
                    dataList.Add(rawData);
                }

                return dataList;
            }
        }

        /// <summary>
        /// Creates a map from the actual column names in the file to the corresponding 
        /// PropertyInfo object of the LabData class.
        /// </summary>
        private Dictionary<int, PropertyInfo> BuildPropertyMap(string[] actualHeaders)
        {
            var map = new Dictionary<int, PropertyInfo>();
            var labDataType = typeof(LabData);

            for (int i = 0; i < actualHeaders.Length; i++)
            {
                // Clean the header name (e.g., remove spaces or convert case if needed)
                string headerName = actualHeaders[i].Replace("_", ""); // PATIENT_ID -> PATIENTID

                // Try to find a property on LabData that matches the header name (case-insensitive)
                var property = labDataType.GetProperty(
                    headerName,
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                if (property != null)
                {
                    map.Add(i, property);
                }
                else
                {
                    // Optional: Throw an error if a header doesn't match a property
                    throw new InvalidOperationException($"Header '{actualHeaders[i]}' does not match any property in the LabData model.");
                }
            }
            return map;
        }

        /// <summary>
        /// Populates a new LabData object using the field values and the property map.
        /// </summary>
        private LabData MapFieldsToLabData(string[] fields, Dictionary<int, PropertyInfo> propertyMap)
        {
            var rawData = new LabData();

            foreach (var kvp in propertyMap)
            {
                int index = kvp.Key;        // Index of the field in the current line
                PropertyInfo property = kvp.Value; // The C# property to set

                // Set the property value (string) from the corresponding field index
                property.SetValue(rawData, fields[index]);
            }

            return rawData;
        }
    }
}
