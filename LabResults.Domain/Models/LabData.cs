namespace LabResults.Domain.Models
{
    public class LabData
    {
        // Sample fields
        public string ClinicNo { get; set; }
        public string Barcode { get; set; }
        public string CollectionDate { get; set; }
        public string CollectionTime { get; set; }

        // Patient fields
        public string PatientId { get; set; }
        public string PatientName { get; set; }
        public string DateOfBirth { get; set; }
        public string Gender { get; set; }

        // TestResult fields
        public string TestCode { get; set; }
        public string TestName { get; set; }
        public string Result { get; set; }
        public string Unit { get; set; }
        public string RefRangeLow { get; set; }
        public string RefRangeHigh { get; set; }
        public string Note { get; set; }
        public string NonSpecRefs { get; set; }
    }
}
