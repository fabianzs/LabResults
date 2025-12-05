namespace LabResults.Domain.Models
{
    public class TestResultModel
    {
        public long Barcode { get; set; }
        public int ClinicNo { get; set; }
        public DateOnly CollectionDate { get; set; }
        public TimeOnly CollectionTime { get; set; }
        public string TestCode { get; set; }
        public string TestName { get; set; }
        public string Result { get; set; }
        public string Unit { get; set; }
        public decimal? RefRangeLow { get; set; }
        public decimal? RefRangeHigh { get; set; }
        public string Note { get; set; }
        public string NonSpecRefs { get; set; }
    }
}
