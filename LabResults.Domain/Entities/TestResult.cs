namespace LabResults.Domain.Entities
{
    public class TestResult
    {
        public int Id { get; set; }

        public string TestCode { get; set; }
        public string TestName { get; set; }
        public string Result { get; set; }
        public string Unit { get; set; }
        public decimal? RefRangeLow { get; set; }
        public decimal? RefRangeHigh { get; set; }
        public string Note { get; set; }
        public string NonSpecRefs { get; set; }

        public int SampleId { get; set; }
    }
}
