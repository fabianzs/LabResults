using Labresults.Infrastructure.Persistence;
using LabResults.Domain;
using LabResults.Domain.Interfaces;
using LabResults.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Labresults.Infrastructure.Readers
{
    public class TestResultReader : ITestResultReader
    {
        private readonly LabResultsDbCotext _context;

        public TestResultReader(LabResultsDbCotext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TestResultModel>> GetTestResultsByPatientIdAsync(int patientId, CancellationToken cancellationToken)
        {
            var testResults = await _context.Patients
                    .Where(p => p.Id == patientId)
                    .SelectMany(p => p.Samples)
                    .SelectMany(s => s.TestResults, (s, tr) => new { Sample = s, TestResult = tr })
                    .Select(x => new TestResultModel
                    {
                        ClinicNo = x.Sample.ClinicNo,
                        Barcode = x.Sample.Barcode,
                        CollectionDate = x.Sample.CollectionDate,
                        CollectionTime = x.Sample.CollectionTime,
                        TestCode = x.TestResult.TestCode,
                        TestName = x.TestResult.TestName,
                        Result = x.TestResult.Result,
                        Unit = x.TestResult.Unit,
                        RefRangeLow = x.TestResult.RefRangeLow,
                        RefRangeHigh = x.TestResult.RefRangeHigh,
                        Note = x.TestResult.Note,
                        NonSpecRefs = x.TestResult.NonSpecRefs
                    }).ToListAsync(cancellationToken);

            if(testResults == null || testResults.Count == 0)
            {
                throw new NotFoundException($"No test results found for patient with ID {patientId}.");
            }

            return testResults;
        }
    }
}
