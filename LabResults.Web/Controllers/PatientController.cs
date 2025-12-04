using LabResults.Domain.Interfaces;
using LabResults.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace LabResults.Web.Controllers
{
    [ApiController]
    [Route("api/patients")]
    public class PatientController : ControllerBase
    {
        private readonly IPatientReader _patientReader;

        public PatientController(IPatientReader patientReader)
        {
            _patientReader = patientReader;
        }

        // GET /patients
        [HttpGet]
        public async Task<IEnumerable<PatientModel>> GetPatientsAsync(CancellationToken cancellationToken)
        {
            return await _patientReader.GetPatientsAsync(cancellationToken);
        }

        // GET /patients/{patientId}
        [HttpGet("{patientId:int}")]
        public async Task<PatientModel> GetPatientAsync(int patientId, CancellationToken cancellationToken)
        {
            return await _patientReader.GetPatientAsync(patientId, cancellationToken);
        }
    }
}
