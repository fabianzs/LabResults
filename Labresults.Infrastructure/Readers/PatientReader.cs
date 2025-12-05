using Labresults.Infrastructure.Persistence;
using LabResults.Domain;
using LabResults.Domain.Interfaces;
using LabResults.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Labresults.Infrastructure.Readers
{
    public class PatientReader : IPatientReader
    {
        private readonly LabResultsDbCotext _context;

        public PatientReader(LabResultsDbCotext context)
        {
            _context = context;
        }

        public async Task<PatientModel> GetPatientAsync(int id, CancellationToken cancellationToken)
        {
            var patient = await _context.Patients.Select(p => new PatientModel
            {
                PatientId = p.Id,
                PatientName = p.PatientName,
                DateOfBirth = p.DateOfBirth,
                Gender = p.Gender,
            }).FirstOrDefaultAsync(p => p.PatientId == id, cancellationToken);

            if (patient == null)
            {
                throw new NotFoundException($"Patient with ID {id} not found.");
            }

            return patient;
        }

        public async Task<IEnumerable<PatientModel>> GetPatientsAsync(CancellationToken cancellationToken)
        {
            return await _context.Patients.Select(p => new PatientModel
            {
                PatientId = p.Id,
                PatientName =  p.PatientName,
                DateOfBirth = p.DateOfBirth,
                Gender = p.Gender
            }).OrderBy(p => p.PatientName).ToListAsync(cancellationToken);
        }
    }
}
