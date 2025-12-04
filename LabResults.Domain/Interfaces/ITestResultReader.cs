using LabResults.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabResults.Domain.Interfaces
{
    public interface ITestResultReader
    {
        Task<IEnumerable<TestResultModel>> GetTestResultsByPatientIdAsync(int patientId, CancellationToken cancellationToken);
    }
}
