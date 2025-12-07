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
                        continue;
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

            var actualHeaderMap = actualHeaders
                .Select((header, index) => new { Header = header.Trim().ToUpperInvariant(), Index = index })
                .ToDictionary(x => x.Header, x => x.Index);

            for (int i = 0; i < _expectedHeaders.Length; i++)
            {
                string expectedHeader = _expectedHeaders[i];
                string expectedKey = expectedHeader.Trim().ToUpperInvariant();

                if (!actualHeaderMap.TryGetValue(expectedKey, out int actualIndex))
                {
                    throw new InvalidOperationException($"Header validation failed: Expected header '{expectedHeader}' was not found in the data file.");
                }

                string propertyName = expectedHeader.Replace("_", "");

                var property = labDataType.GetProperty(
                    propertyName,
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                if (property != null)
                {
                    map.Add(actualIndex, property);
                }
                else
                {
                    throw new InvalidOperationException($"Configuration Error: Expected header '{expectedHeader}' does not match any property in the LabData model.");
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
