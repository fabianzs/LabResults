using LabResults.DataLoader;
using LabResults.Domain.Interfaces;
using LabResults.Domain.Models;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace Labresults.Infrastructure.Readers
{
    public class LabDataReader : IDataReader
    {
        private readonly string[] _expectedHeaders;

        public LabDataReader(IOptions<LabFileSettings> settings)
        {
            _expectedHeaders = settings.Value.ExpectedHeaders;
        }

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

                    headerLine = line;
                    break;
                }

                if (headerLine == null)
                {
                    throw new InvalidOperationException("File contains no data or header line.");
                }

                // 2. Validate the Header
                var actualHeaders = headerLine.Split('|', StringSplitOptions.TrimEntries);

                if (actualHeaders.Length != _expectedHeaders.Length)
                {
                    throw new InvalidOperationException($"Header validation failed: Expected {_expectedHeaders.Length} columns but found {actualHeaders.Length}.");
                }

                var propertyMap = BuildPropertyMap(actualHeaders);

                for (int i = 0; i < _expectedHeaders.Length; i++)
                {
                    if (!string.Equals(actualHeaders[i], _expectedHeaders[i], StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException($"Header validation failed: Column at index {i} mismatch. Expected '{_expectedHeaders[i]}' but found '{actualHeaders[i]}'.");
                    }
                }

                var dataList = new List<LabData>();

                // 3. Process data lines
                while (enumerator.MoveNext())
                {
                    var line = enumerator.Current;
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var fields = line.Split('|');
                    if (fields.Length != actualHeaders.Length) continue;

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
                string headerName = actualHeaders[i].Replace("_", "");

                var property = labDataType.GetProperty(
                    headerName,
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                if (property != null)
                {
                    map.Add(i, property);
                }
                else
                {
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
                int index = kvp.Key;
                PropertyInfo property = kvp.Value;

                property.SetValue(rawData, fields[index]);
            }

            return rawData;
        }
    }
}
