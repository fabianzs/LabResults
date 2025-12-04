using LabResults.Domain.Models;
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
        public IEnumerable<LabData> GetLabresults()
        {
            return new List<LabData>();
        }
    }
}
