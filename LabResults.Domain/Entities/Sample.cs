namespace LabResults.Domain.Entities
{
    public class Sample
    {
        public int Id { get; set; }

        public long Barcode { get; set; }
        public int ClinicNo { get; set; }
        public DateOnly CollectionDate { get; set; }
        public TimeOnly CollectionTime { get; set; }

        public int PatientId { get; set; }

        public ICollection<TestResult> TestResults { get; set; } = new List<TestResult>();
    }
}
