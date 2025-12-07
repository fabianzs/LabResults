using Labresults.Infrastructure.Readers;
using LabResults.DataLoader;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace LabResults.Tests
{
    public class LabDataReaderTests : IDisposable
    {
        private readonly string _tempFilePath;
        private readonly Mock<IOptions<LabFileSettings>> _mockOptions;
        private readonly LabDataReader _reader;
        private readonly string[] _defaultExpectedHeaders = new[]
        {
            "CLINIC_NO", "BARCODE", "PATIENT_ID", "PATIENT_NAME", "DOB", "GENDER",
            "COLLECTIONDATE", "COLLECTIONTIME", "TESTCODE", "TESTNAME", "RESULT",
            "UNIT", "REFRANGELOW", "REFRANGEHIGH", "NOTE", "NONSPECREFS"
        };

        public LabDataReaderTests()
        {
            _tempFilePath = Path.GetTempFileName();

            _mockOptions = new Mock<IOptions<LabFileSettings>>();
            _mockOptions.Setup(o => o.Value).Returns(new LabFileSettings
            {
                ExpectedHeaders = _defaultExpectedHeaders
            });

            _reader = new LabDataReader(_mockOptions.Object);
        }

        public void Dispose()
        {
            if (File.Exists(_tempFilePath))
            {
                File.Delete(_tempFilePath);
            }
        }


        private void WriteFileContent(string content)
        {
            File.WriteAllText(_tempFilePath, content);
        }

        private string GetValidDataLine()
        {
            return "1|123456789|101|John Doe|2000-01-01|M|2023-10-26|08:30|GLU|Glucose|120|mg/dL|70|100|Normal|";
        }


        [Fact]
        public async Task ReadDataFromFileAsync_FileDoesNotExist_ThrowsFileNotFoundException()
        {
            // Arrange
            string nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".txt");

            // Act & Assert
            await Assert.ThrowsAsync<FileNotFoundException>(() =>
                _reader.ReadDataFromFileAsync(nonExistentPath));
        }

        [Fact]
        public async Task ReadDataFromFileAsync_WhenFileOnlyContainsHeader_ReturnsEmptyList()
        {
            // Arrange
            WriteFileContent(string.Join("|", _defaultExpectedHeaders));

            // Act
            var result = await _reader.ReadDataFromFileAsync(_tempFilePath);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task ReadDataFromFileAsync_ValidFile_ReturnsCorrectlyMappedData()
        {
            // Arrange
            string content = string.Join("|", _defaultExpectedHeaders) + Environment.NewLine + GetValidDataLine();
            WriteFileContent(content);

            // Act
            var result = (await _reader.ReadDataFromFileAsync(_tempFilePath))?.ToList();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            var data = result.First();

            Assert.Equal("1", data.ClinicNo);
            Assert.Equal("123456789", data.Barcode);
            Assert.Equal("John Doe", data.PatientName);
            Assert.Equal("120", data.Result);
            Assert.Equal("100", data.RefRangeHigh);
        }

        [Fact]
        public async Task ReadDataFromFileAsync_WhenHeadersAreInMixedOrder_ReturnsCorrectlyMappedData()
        {
            // Arrange: Reverse the order of the first few headers
            var reversedHeaders = _defaultExpectedHeaders.Reverse().ToArray();

            var validFields = GetValidDataLine().Split('|');
            var fieldMap = new Dictionary<string, string>();
            for (int i = 0; i < _defaultExpectedHeaders.Length; i++)
            {
                fieldMap[_defaultExpectedHeaders[i]] = validFields[i];
            }

            // Create a new header array and construct the data line accordingly
            string[] newHeaders = { "BARCODE", "PATIENT_ID", "CLINIC_NO", "TESTNAME" /*, ... rest of the 16 headers */ };
            string[] newFields = newHeaders.Select(h => fieldMap[h]).ToArray();

            var fullNewHeaders = newHeaders.Concat(_defaultExpectedHeaders.Except(newHeaders)).ToArray();
            var fullNewFields = fullNewHeaders.Select(h => fieldMap[h]).ToArray();

            string content = string.Join("|", fullNewHeaders) + Environment.NewLine + string.Join("|", fullNewFields);
            WriteFileContent(content);

            // Act
            var result = (await _reader.ReadDataFromFileAsync(_tempFilePath))?.ToList();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            var data = result.First();

            // Check values that were moved
            Assert.Equal("123456789", data.Barcode);
            Assert.Equal("101", data.PatientId);
            Assert.Equal("1", data.ClinicNo);
            Assert.Equal("Glucose", data.TestName);
        }

        [Fact]
        public async Task ReadDataFromFileAsync_ValidFileWithComments_SkipsCommentsAndEmptyLines()
        {
            // Arrange
            string validHeader = string.Join("|", _defaultExpectedHeaders);
            string content =
                "#" + Environment.NewLine +
                "## This is a comment" + Environment.NewLine +
                "" + Environment.NewLine +
                validHeader + Environment.NewLine +
                GetValidDataLine();
            WriteFileContent(content);

            // Act
            var result = (await _reader.ReadDataFromFileAsync(_tempFilePath))?.ToList();

            // Assert
            Assert.Single(result);
        }

       
        [Fact]
        public async Task ReadDataFromFileAsync_MissingHeader_ThrowsInvalidOperationException()
        {
            // Arrange
            string content = GetValidDataLine();
            WriteFileContent(content);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _reader.ReadDataFromFileAsync(_tempFilePath));
        }

        [Fact]
        public async Task ReadDataFromFileAsync_MissingExpectedHeader_ThrowsInvalidOperationExeption()
        {
            // Arrange
            var incompleteHeaders = _defaultExpectedHeaders.Where(h => h != "BARCODE").ToArray();
            string content = string.Join("|", incompleteHeaders) + Environment.NewLine + "data";
            WriteFileContent(content);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _reader.ReadDataFromFileAsync(_tempFilePath));

            Assert.Contains("Header validation failed: Expected 16 columns but found 15.", ex.Message);
        }


        [Fact]
        public async Task ReadDataFromFileAsync_ExtraHeader_ThrowsInvalidOperationException()
        {
            string content = string.Join("|", _defaultExpectedHeaders) + "|EXTRA_COLUMN" + Environment.NewLine + GetValidDataLine();
            WriteFileContent(content);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _reader.ReadDataFromFileAsync(_tempFilePath));

            Assert.Contains("Header validation failed: Expected 16 columns but found 17.", ex.Message);
        }

        [Fact]
        public async Task ReadDataFromFileAsync_HeaderNameMismatch_ThrowsInvalidOperationException()
        {
            // Arrange: Change one header name to be incorrect (e.g., PatientName -> P_NAME)
            var badHeaders = _defaultExpectedHeaders.ToArray();
            badHeaders[3] = "P_NAME";
            string content = string.Join("|", badHeaders) + Environment.NewLine + GetValidDataLine();
            WriteFileContent(content);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _reader.ReadDataFromFileAsync(_tempFilePath));

            Assert.Contains("Header validation failed: Expected header 'PATIENT_NAME' was not found in the data file.", ex.Message);
        }


        [Fact]
        public async Task ReadDataFromFileAsync_SkipsDataLineWithWrongColumnCount()
        {
            // Arrange
            string content = string.Join("|", _defaultExpectedHeaders) + Environment.NewLine +
                             "1|123456789|101|John Doe"; // Only 4 columns
            WriteFileContent(content);

            // Act
            var result = (await _reader.ReadDataFromFileAsync(_tempFilePath))?.ToList();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void BuildPropertyMap_UnknownHeader_ThrowsInvalidOperationException()
        {
            // Arrange
            var privateReader = new LabDataReader(_mockOptions.Object);
            var actualHeaders = new[] { "CLINIC_NO", "NON_EXISTENT_FIELD" };

            // Use reflection to get the private method
            var methodInfo = typeof(LabDataReader).GetMethod("BuildPropertyMap", BindingFlags.NonPublic | BindingFlags.Instance);

            // Act & Assert
            var ex = Assert.Throws<TargetInvocationException>(() =>
                methodInfo.Invoke(privateReader, new object[] { actualHeaders }));

            Assert.NotNull(ex.InnerException);
            Assert.IsType<InvalidOperationException>(ex.InnerException);
            Assert.Contains("Header validation failed: Expected header 'BARCODE' was not found in the data file.", ex.InnerException.Message);
        }
    }
}
