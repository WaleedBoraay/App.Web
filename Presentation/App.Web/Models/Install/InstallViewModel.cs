using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace App.Web.Models.Install
{
    public class InstallViewModel
    {
        // Database
        [Required]
        public string DataProvider { get; set; } = "SqlServer";

        [Required]
        [Display(Name = "Connection String")]
        public string ConnectionString { get; set; }

        public bool CreateDatabaseIfNotExists { get; set; } = false;

        // Admin
        [Required, EmailAddress]
        public string AdminEmail { get; set; } = "admin@yourStore.com";

        [Required, DataType(DataType.Password)]
        public string AdminPassword { get; set; }

        // Sample data
        public bool LoadSampleData { get; set; } = false;

        // UI helpers
        public List<SelectListItem> AvailableDataProviders { get; set; } = new();
        public List<SelectListItem> AvailableCountries { get; set; } = new();
    }
}
