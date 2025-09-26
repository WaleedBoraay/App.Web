namespace App.Core.Domain.Ref
{
    /// <summary>
    /// Lookup for institution business scale ranges (per BRD Appendix).
    /// </summary>
    public class BusinessScaleRange : BaseEntity
    {
        public string Name { get; set; }
        public int MinValue { get; set; }
        public int MaxValue { get; set; }
        public string RangeLabel { get; set; }
    }
}
