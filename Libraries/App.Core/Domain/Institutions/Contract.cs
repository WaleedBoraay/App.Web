namespace App.Core.Domain.Institutions
{
    /// <summary>
    /// Contract type entity (per Appendix).
    /// </summary>
    public class Contract : BaseEntity
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public bool IsActive { get; set; }
    }
}
