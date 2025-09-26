using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using App.Core.MigratorServices;
using App.Core.Configuration; // DataProviderType

namespace App.Web.Models.Install
{
    /// <summary>
    /// Conditional validation (raw vs built) + nullable optionals to avoid unintended "required".
    /// </summary>
    public class InstallModel : IValidatableObject
    {
        // ===== Database provider & connection =====
        [Required]
        public DataProviderType DataProvider { get; set; } = DataProviderType.SqlServer;

        public bool ConnectionStringRaw { get; set; } = false;

        [Display(Name = "Connection String")]
        public string? ConnectionString { get; set; }

        public string? ServerName { get; set; }
        public string? DatabaseName { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }

        public bool CreateDatabaseIfNotExists { get; set; } = false;

        // Never required; UI hidden. Keep nullable and skip validation
        [ValidateNever]
        public string? Collation { get; set; }

        // ===== Admin =====
        [Required, EmailAddress]
        public string AdminEmail { get; set; } = "admin@yourStore.com";

        [Required, DataType(DataType.Password)]
        public string AdminPassword { get; set; } = "";

        // ===== Sample data =====
        public bool LoadSampleData { get; set; } = false;

        // ===== UI helpers =====
        public List<SelectListItem> AvailableLanguages { get; set; } = new();
        public List<SelectListItem> AvailableCountries { get; set; } = new();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ConnectionStringRaw)
            {
                if (string.IsNullOrWhiteSpace(ConnectionString))
                    yield return new ValidationResult("ConnectionString is required.", new[] { nameof(ConnectionString) });
                yield break;
            }

            if (string.IsNullOrWhiteSpace(ServerName))
                yield return new ValidationResult("ServerName is required.", new[] { nameof(ServerName) });
            if (string.IsNullOrWhiteSpace(DatabaseName))
                yield return new ValidationResult("DatabaseName is required.", new[] { nameof(DatabaseName) });
        }
    }
}
