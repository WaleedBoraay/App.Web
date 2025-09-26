using App.Core.Domain.Common;

namespace App.Core.Domain.Reports
{
    public partial class Dashboard : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}