using App.Core.Domain.Common;

namespace App.Core.Domain.Directory
{
    public partial class WorkingLocation : BaseEntity
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public int CountryId { get; set; }
    }
}