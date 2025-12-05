using LabResults.Domain.Models;

namespace LabResults.Domain.Interfaces
{
    public interface IPatientReader
    {
        Task<IEnumerable<PatientModel>> GetPatientsAsync(CancellationToken cancellationToken);

        Task<PatientModel> GetPatientAsync(int Id, CancellationToken cancellationToken);
    }
}
