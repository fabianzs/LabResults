namespace LabResults.Domain.Models
{
    public class PatientModel
    {
        public int PatientId { get; set; }
        public string PatientName { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public string Gender { get; set; }

        public IEnumerable<TestResultModel>? TestResults { get; set; }
    }
}
