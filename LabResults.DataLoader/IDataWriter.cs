using LabResults.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabResults.DataLoader
{
    internal interface IDataWriter
    {
        Task ProcessAndSaveDataAsync(List<LabData> rawDataList);
    }
}
