using App.Web.Infrastructure.Mapper.BaseModel;

namespace App.Web.Areas.Admin.Models
{
    public record class StateProvinceModel : BaseAppEntityModel
    {
        public int CountryId { get; set; }

        public string Name { get; set; }

        public string Abbreviation { get; set; }

        public bool Published { get; set; }

        public int DisplayOrder { get; set; }
    }
}
