using LabResults.Domain.Models;

namespace LabResults.Domain.Interfaces
{
    public interface IDataWriter
    {
        Task ProcessAndSaveDataAsync(List<LabData> rawDataList);
    }
}
