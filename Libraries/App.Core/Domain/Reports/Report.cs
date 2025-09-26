using App.Core.Domain.Common;

namespace App.Core.Domain.Reports
{
    public partial class Report : BaseEntity
    {
        public string Title { get; set; }
        public string ReportType { get; set; }
        public string FilePath { get; set; }
        public DateTime CreatedOnUtc { get; set; }
    }
}