using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabResults.Domain.Models
{
    public class PatientModel
    {
        public int PatientId { get; set; }
        public string PatientName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }

        public IEnumerable<TestResultModel> TestResults { get; set; } = new List<TestResultModel>();
    }
}
