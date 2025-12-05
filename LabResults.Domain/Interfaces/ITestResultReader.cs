using LabResults.Domain.Models;

namespace LabResults.Domain.Interfaces
{
    public interface ITestResultReader
    {
        Task<IEnumerable<TestResultModel>> GetTestResultsByPatientIdAsync(int patientId, CancellationToken cancellationToken);
    }
}
