using Xunit;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Labresults.Infrastructure.Persistence;
using Labresults.Infrastructure.Readers;
using LabResults.Domain.Entities;
using LabResults.Domain;
using System.Collections.Generic;

namespace LabResults.Tests
{
    public class TestResultReaderTests : IDisposable
    {
        private readonly LabResultsDbCotext _context;
        private readonly TestResultReader _reader;

        private const int PatientIdWithResults = 10;
        private const int PatientIdWithoutResults = 20;
        private const int NonExistentPatientId = 99;

        private const int ClinicNo1 = 101;
        private const int ClinicNo2 = 102;
        private const long Barcode1 = 100000000001;
        private const long Barcode2 = 100000000002;
        private const long Barcode3 = 200000000001;

        public TestResultReaderTests()
        {
            var options = new DbContextOptionsBuilder<LabResultsDbCotext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new LabResultsDbCotext(options);
            SeedDatabase();
            _reader = new TestResultReader(_context);
        }

        private void SeedDatabase()
        {
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            // --- Patient 10 (With Results) ---
            var patient10 = new Patient { Id = PatientIdWithResults, Gender = "M", PatientName = "Test patient1", DateOfBirth = new DateOnly(1988, 08, 06) };
            var sample1A = new Sample
            {
                Id = 1,
                PatientId = PatientIdWithResults,
                Barcode = Barcode1,
                ClinicNo = ClinicNo1,
                CollectionDate = new DateOnly(2024, 1, 15),
                CollectionTime = new TimeOnly(8, 30, 0),
                TestResults = new List<TestResult>
            {
                new TestResult { Id = 101, TestCode = "GLU", TestName = "Glucose", Result = "95", Unit = "mg/dL", RefRangeLow = 70, NonSpecRefs = "", Note = "" },
                new TestResult { Id = 102, TestCode = "CRE", TestName = "Creatinine", Result = "1.2", Unit = "mg/dL", NonSpecRefs = "", Note = "" }
            }
            };
            var sample1B = new Sample
            {
                Id = 2,
                PatientId = PatientIdWithResults,
                Barcode = Barcode2,
                ClinicNo = ClinicNo1,
                CollectionDate = new DateOnly(2024, 1, 16),
                CollectionTime = new TimeOnly(9, 0, 0),
                TestResults = new List<TestResult>
            {
                new TestResult { Id = 103, TestCode = "HGB", TestName = "Hemoglobin", Result = "14.5", Unit = "g/dL", NonSpecRefs = "", Note = "" }
            }
            };

            // --- Patient 20 (Without Results, only a Sample) ---
            var patient20 = new Patient { Id = PatientIdWithoutResults, Gender = "F", PatientName = "Test patient2", DateOfBirth = new DateOnly(1993, 10, 12) };
            var sample2A = new Sample
            {
                Id = 3,
                PatientId = PatientIdWithoutResults,
                Barcode = Barcode3, 
                ClinicNo = ClinicNo2,
                CollectionDate = new DateOnly(2024, 2, 1),
                CollectionTime = new TimeOnly(10, 0, 0),
                TestResults = new List<TestResult>()
            };

            // --- Patient 30 (Existent, but not queried) ---
            var patient30 = new Patient { Id = 30, Gender = "F", PatientName = "Test patient3", DateOfBirth = new DateOnly(2001, 03, 28) };

            _context.Patients.AddRange(patient10, patient20, patient30);
            _context.Samples.AddRange(sample1A, sample1B, sample2A);
            _context.SaveChanges();
        }


        [Fact]
        public async Task GetTestResultsByPatientIdAsync_ForPatientWithResults_ReturnsAllResults()
        {
            // Act
            var results = (await _reader.GetTestResultsByPatientIdAsync(PatientIdWithResults, CancellationToken.None)).ToList();

            // Assert
            Assert.NotNull(results);
            Assert.Equal(3, results.Count);

            var glucoseResult = results.Single(r => r.TestCode == "GLU");
            Assert.Equal("95", glucoseResult.Result);

            Assert.Equal(Barcode1, glucoseResult.Barcode);
            Assert.Equal(ClinicNo1, glucoseResult.ClinicNo);

            Assert.Equal(new DateOnly(2024, 1, 15), glucoseResult.CollectionDate);
            Assert.Equal(new TimeOnly(8, 30, 0), glucoseResult.CollectionTime);
        }

        [Fact]
        public async Task GetTestResultsByPatientIdAsync_PatientHasNoResults_ThrowsNotFoundException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
                _reader.GetTestResultsByPatientIdAsync(PatientIdWithoutResults, CancellationToken.None));

            Assert.Contains($"No test results found for patient with ID {PatientIdWithoutResults}.", exception.Message);
        }

        [Fact]
        public async Task GetTestResultsByPatientIdAsync_PatientDoesNotExist_ThrowsNotFoundException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
                _reader.GetTestResultsByPatientIdAsync(NonExistentPatientId, CancellationToken.None));

            Assert.Contains($"No test results found for patient with ID {NonExistentPatientId}.", exception.Message);
        }

        [Fact]
        public async Task GetTestResultsByPatientIdAsync_ExcludesResultsFromOtherPatients()
        {
            // Act
            var results = (await _reader.GetTestResultsByPatientIdAsync(PatientIdWithResults, CancellationToken.None)).ToList();

            // Assert
            Assert.Equal(3, results.Count);
            // Ensure Patient 20's sample wasn't included by checking its Barcode
            Assert.DoesNotContain(results, r => r.Barcode == Barcode3);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}