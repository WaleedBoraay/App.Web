using App.Web.Infrastructure.Mapper.BaseModel;

namespace App.Web.Areas.Admin.Models
{
    public record class CountryModel : BaseAppEntityModel
    {
        public string Name { get; set; }
        public string TwoLetterIsoCode { get; set; }
        public string ThreeLetterIsoCode { get; set; }
        public int NumericIsoCode { get; set; }
        public bool Published { get; set; }
        public int DisplayOrder { get; set; }
    }
}
