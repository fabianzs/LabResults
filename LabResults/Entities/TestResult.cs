using System.ComponentModel.DataAnnotations;

namespace LabResults.Entities
{
    public class TestResult
    {
        [Key]
        public int Id { get; set; }

        // Test details only
        public string TestCode { get; set; }
        public string TestName { get; set; }
        public string Result { get; set; }
        public string Unit { get; set; }
        public decimal? RefRangeLow { get; set; }
        public decimal? RefRangeHigh { get; set; }
        public string Note { get; set; }
        public string NonSpecRefs { get; set; }

        // Sample relationship
        public int SampleId { get; set; }
    }
}
