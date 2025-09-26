namespace App.Core.Domain.Ref
{
    /// <summary>
    /// Lookup for institution employee count ranges (per BRD Appendix).
    /// </summary>
    public class EmployeeRange : BaseEntity
    {
        public string Name { get; set; }
        public int MinValue { get; set; }
        public int MaxValue { get; set; }
    }
}
