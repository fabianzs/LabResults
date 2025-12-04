using LabResults.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabResults.Domain.Interfaces
{
    public interface IPatientReader
    {
        Task<IEnumerable<PatientModel>> GetPatientsAsync(CancellationToken cancellationToken);

        Task<PatientModel> GetPatientAsync(int Id, CancellationToken cancellationToken);
    }
}
