namespace App.Core.Domain.Institutions
{
    /// <summary>
    /// Job title entity (per Appendix).
    /// </summary>
    public class Job : BaseEntity
    {
        public string Name { get; set; }
        public bool IsActive { get; set; }
    }
}
