using App.Core.Domain.Localization;
using System.ComponentModel.DataAnnotations;

namespace App.Web.Areas.Admin.Models.Localization
{
    public class ResourceEditModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Resource Name")]
        public string ResourceName { get; set; }

        [Display(Name = "Resource Value")]
        public string ResourceValue { get; set; }

        [Required]
        [Display(Name = "Language")]
        public int LanguageId { get; set; }

        // عشان الـ Dropdown
        public IList<Language> Languages { get; set; } = new List<Language>();
    }
}
