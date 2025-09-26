using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace App.Core.Domain.Users
{
    public class UserPreference : BaseEntity
    {
        public int UserId { get; set; }

        // UI & Localization
        public string Language { get; set; }
        public int? LanguageId { get; set; }

        // Security
        public bool EnableMfa { get; set; }

        // Notifications
        public bool NotifyByEmail { get; set; }
        public bool NotifyBySms { get; set; }
        public bool NotifyInApp { get; set; }

        public DateTime UpdatedOnUtc { get; set; }
    }
}

