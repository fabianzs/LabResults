using Labresults.Infrastructure.Persistence;
using Labresults.Infrastructure.Readers;
using LabResults.Domain;
using LabResults.Domain.Entities;
using LabResults.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace LabResults.Tests
{
    public class PatientReaderTests : IDisposable
    {
        private readonly LabResultsDbCotext _context;
        private readonly PatientReader _reader;

        public PatientReaderTests()
        {
            var options = new DbContextOptionsBuilder<LabResultsDbCotext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new LabResultsDbCotext(options);

            SeedDatabase();

            _reader = new PatientReader(_context);
        }

        private void SeedDatabase()
        {
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            _context.Patients.AddRange(
                new Patient { Id = 1, PatientName = "Alice Smith", DateOfBirth = new DateOnly(1990, 1, 1), Gender = "F" },
                new Patient { Id = 2, PatientName = "Bob Johnson", DateOfBirth = new DateOnly(1985, 5, 15), Gender = "M" },
                new Patient { Id = 3, PatientName = "Charlie Brown", DateOfBirth = new DateOnly(2000, 10, 20), Gender = "M" }
            );
            _context.SaveChanges();
        }


        [Fact]
        public async Task GetPatientAsync_PatientExists_ReturnsPatientModel()
        {
            // Arrange
            int existingId = 2;

            // Act
            var result = await _reader.GetPatientAsync(existingId, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PatientModel>(result);
            Assert.Equal(existingId, result.PatientId);
            Assert.Equal("Bob Johnson", result.PatientName);
        }

        [Fact]
        public async Task GetPatientAsync_PatientDoesNotExist_ThrowsNotFoundException()
        {
            // Arrange
            int nonExistingId = 999;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
                _reader.GetPatientAsync(nonExistingId, CancellationToken.None));

            Assert.Contains($"Patient with ID {nonExistingId} not found.", exception.Message);
        }

        [Fact]
        public async Task GetPatientsAsync_ExistingData_ReturnsAllPatients()
        {
            // Act
            var results = (await _reader.GetPatientsAsync(CancellationToken.None)).ToList();

            // Assert
            Assert.NotNull(results);
            Assert.Equal(3, results.Count);

            Assert.Equal("Alice Smith", results[0].PatientName);
            Assert.Equal("Bob Johnson", results[1].PatientName);
            Assert.Equal("Charlie Brown", results[2].PatientName);
        }

        [Fact]
        public async Task GetPatientsAsync_ExistingData_ReturnsCorrectPatientModels()
        {
            // Act
            var results = (await _reader.GetPatientsAsync(CancellationToken.None)).ToList();
            var charlie = results.Single(p => p.PatientId == 3);

            Assert.Equal(3, charlie.PatientId);
            Assert.Equal("Charlie Brown", charlie.PatientName);
            Assert.Equal(new DateOnly(2000, 10, 20), charlie.DateOfBirth);
            Assert.Equal("M", charlie.Gender);
        }


        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}