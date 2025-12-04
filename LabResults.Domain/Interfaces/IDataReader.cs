using LabResults.Domain.Models;

namespace LabResults.Domain.Interfaces
{
    public interface IDataReader
    {
        Task<IEnumerable<LabData>?> ReadDataFromFileAsync(string filePath);
    }
}
