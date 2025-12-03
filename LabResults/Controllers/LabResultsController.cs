using LabResults.Models;
using Microsoft.AspNetCore.Mvc;

namespace LabResults.Controllers
{
    [ApiController]
    [Route("api/labresults")]
    public class LabResultsController : ControllerBase
    {
       private readonly ILogger<LabResultsController> _logger;

        public LabResultsController(ILogger<LabResultsController> logger)
        {
            _logger = logger;
        }

        // GET /labresults
        [HttpGet]
        public IEnumerable<LabResult> GetLabresults()
        {
            return new List<LabResult>();
        }
    }
}
