namespace LabResults.Domain.Entities
{
    public class Sample
    {
        public int Id { get; set; }

        // Sample/Collection identifiers
        public long Barcode { get; set; }
        public int ClinicNo { get; set; }
        public DateTime CollectionDate { get; set; }
        public TimeSpan CollectionTime { get; set; }

        // Patient relationship
        public int PatientId { get; set; }

        // Test results for this sample
        public ICollection<TestResult> TestResults { get; set; } = new List<TestResult>();
    }
}
