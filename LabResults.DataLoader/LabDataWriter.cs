using Labresults.Infrastructure.Persistence;
using LabResults.Domain.Entities;
using LabResults.Domain.Interfaces;
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
                var patient = _context.Patients.Local.FirstOrDefault(p => p.Id == patientId);
                if (patient == null)
                {
                    patient = await _context.Patients
                        .SingleOrDefaultAsync(p => p.Id == patientId);
                }

                if (patient == null)
                {
                    patient = new Patient
                    {
                        Id = patientId,
                        PatientName = rawData.PatientName,
                        DateOfBirth = DateOnly.ParseExact(rawData.DOB, DateFormat, CultureInfo.InvariantCulture),
                        Gender = rawData.Gender
                    };
                    _context.Patients.Add(patient);
                }

                //Find or Create Sample (using Barcode)
                var sample = _context.Samples.Local.FirstOrDefault(s => s.Barcode == barcode);
                if(sample == null)
                {
                    sample = await _context.Samples.SingleOrDefaultAsync(s => s.Barcode == barcode);
                }

                if (sample == null)
                {
                    sample = new Sample
                    {
                        Barcode = barcode,
                        ClinicNo = int.Parse(rawData.ClinicNo),
                        CollectionDate = DateOnly.ParseExact(rawData.CollectionDate, DateFormat),
                        CollectionTime = TimeOnly.ParseExact(rawData.CollectionTime, TimeFormat, CultureInfo.InvariantCulture),
                        PatientId = patient.Id
                    };
                    patient.Samples.Add(sample);
                }

                //TestResult
                var existingResult = sample.TestResults.FirstOrDefault(tr => tr.TestCode == rawData.TestCode);

                // If not found in the local, in-memory collection, check the database.
                if (existingResult == null)
                {
                    existingResult = await _context.TestResults
                        .SingleOrDefaultAsync(tr => sample.Barcode == barcode &&
                                                    tr.TestCode == rawData.TestCode);
                }

                if (existingResult != null)
                {
                    // --- UPDATE existing result ---
                    // The result exists either in memory or in the database. Update its fields.
                    existingResult.Result = rawData.Result;
                    existingResult.Unit = rawData.Unit;
                    existingResult.RefRangeLow = ParseNullableDecimal(rawData.RefRangeLow);
                    existingResult.RefRangeHigh = ParseNullableDecimal(rawData.RefRangeHigh);
                    existingResult.Note = rawData.Note;
                    existingResult.NonSpecRefs = rawData.NonSpecRefs;

                    // EF Core's Change Tracker is now handling the 'Modified' state.
                }
                else
                {
                    // --- CREATE new result ---
                    var testResult = new TestResult
                    {
                        TestCode = rawData.TestCode,
                        TestName = rawData.TestName,
                        Result = rawData.Result,
                        Unit = rawData.Unit,
                        RefRangeLow = ParseNullableDecimal(rawData.RefRangeLow),
                        RefRangeHigh = ParseNullableDecimal(rawData.RefRangeHigh),
                        Note = rawData.Note,
                        NonSpecRefs = rawData.NonSpecRefs,
                        SampleId = sample.Id // This may be 0, but EF Core will fill it on SaveChanges
                    };
                    // Add the new result to the Sample's collection, ensuring it's tracked.
                    sample.TestResults.Add(testResult);
                }
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
