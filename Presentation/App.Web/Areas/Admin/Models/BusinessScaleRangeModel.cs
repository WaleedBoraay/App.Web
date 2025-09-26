namespace App.Web.Areas.Admin.Models
{
    public partial class BusinessScaleRangeModel
    {
        public int BusinessScaleRangeId { get; set; }
        public string RangeLabel { get; set; }
        public int MinValue { get; set; }
        public int MaxValue { get; set; }
    }
}
