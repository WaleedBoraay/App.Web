using System;

namespace App.Core.Domain.Institutions
{
    /// <summary>
    /// Official duty assigned to an employee (per Appendix).
    /// </summary>
    public class Duty : BaseEntity
    {
        public int EmployeeId { get; set; }
        public string DutyType { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Notes { get; set; }
    }
}
