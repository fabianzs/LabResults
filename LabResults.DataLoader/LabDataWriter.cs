using Labresults.Infrastructure.Persistence;
using LabResults.Domain.Entities;
using LabResults.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace LabResults.DataLoader
{
    public class LabDataWriter : IDataWriter
    {
        const string DateFormat = "yyyy-MM-dd";
        const string TimeFormat = "hh\\:mm";
        private readonly LabResultsDbCotext _context;

        public LabDataWriter(LabResultsDbCotext context)
        {
             _context = context;   
        }

        public async Task ProcessAndSaveDataAsync(List<LabData> rawDataList)
        {
            foreach (var rawData in rawDataList)
            {
                if (!int.TryParse(rawData.PatientId, out int patientId)) continue;
                if (!long.TryParse(rawData.Barcode, out long barcode)) continue;

                //Find or Create Patient
                var patient = await _context.Patients
                    .SingleOrDefaultAsync(p => p.Id == patientId);

                if (patient == null)
                {
                    patient = new Patient
                    {
                        Id = patientId,
                        PatientName = rawData.PatientName,
                        DateOfBirth = DateTime.ParseExact(rawData.DateOfBirth, DateFormat, CultureInfo.InvariantCulture),
                        Gender = rawData.Gender
                    };
                    _context.Patients.Add(patient);
                }

                //Find or Create Sample (using Barcode)
                var sample = await _context.Samples
                    .SingleOrDefaultAsync(s => s.Barcode == barcode);

                if (sample == null)
                {
                    sample = new Sample
                    {
                        Barcode = barcode,
                        ClinicNo = int.Parse(rawData.ClinicNo), // Assuming ClinicNo is always valid
                        CollectionDate = DateTime.ParseExact(rawData.CollectionDate, DateFormat, CultureInfo.InvariantCulture),
                        CollectionTime = TimeSpan.ParseExact(rawData.CollectionTime, TimeFormat, CultureInfo.InvariantCulture),
                        PatientId = patient.Id
                    };
                    patient.Samples.Add(sample);
                }

                //TestResult
                var testResult = new TestResult
                {
                    TestCode = rawData.TestCode,
                    TestName = rawData.TestName,
                    Result = rawData.Result,
                    Unit = rawData.Unit,

                    // Re-use the helper for parsing nullable decimals
                    RefRangeLow = ParseNullableDecimal(rawData.RefRangeLow),
                    RefRangeHigh = ParseNullableDecimal(rawData.RefRangeHigh),

                    Note = rawData.Note,
                    NonSpecRefs = rawData.NonSpecRefs,
                    SampleId = sample.Id
                };
                sample.TestResults.Add(testResult);
            }

            await _context.SaveChangesAsync();
        }

        // (Keep the ParseNullableDecimal helper function)
        private decimal? ParseNullableDecimal(string field)
        {
            if (string.IsNullOrWhiteSpace(field))
            {
                return null;
            }
            if (decimal.TryParse(field, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal value))
            {
                return value;
            }
            return null;
        }
    }
}
