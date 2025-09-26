using App.Web.Infrastructure.Mapper.BaseModel;

namespace App.Web.Areas.Admin.Models
{
    public record class LanguageModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string LanguageCulture { get; set; }
        public string UniqueSeoCode { get; set; }
        public string FlagImageFileName { get; set; }
        public bool Rtl { get; set; }
        public bool Published { get; set; }
        public int DisplayOrder { get; set; }
    }
}
