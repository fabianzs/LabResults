using LabResults.Domain.Models;

namespace LabResults
{
    public interface IDataReader
    {
        Task<IEnumerable<LabData>?> ReadDataFromFileAsync(string filePath);
    }
}
