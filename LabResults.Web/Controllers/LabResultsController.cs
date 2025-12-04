using LabResults.Domain.Interfaces;
using LabResults.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace LabResults.Controllers
{
    [ApiController]
    [Route("api/patients/{patientId:int}")]
    public class LabResultsController : ControllerBase
    {
        private readonly ILogger<LabResultsController> _logger;
        private readonly ITestResultReader _testResultReader;

        public LabResultsController(ILogger<LabResultsController> logger)
        {
            _logger = logger;
        }

     
        [HttpGet("labresults")]
        public async Task<IEnumerable<TestResultModel>> GetLabresults(int patientId, CancellationToken cancellationToken)
        {
            return await _testResultReader.GetTestResultsByPatientIdAsync(patientId, cancellationToken);
        }
    }
}
