using App.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core.Domain.Registrations
{
    public enum DocumentType
    {
        [AppResourceDisplayName("DocumentType.Passport")]
        Passport = 1,
        [AppResourceDisplayName("DocumentType.CivilId")]
        CivilId = 2,
        [AppResourceDisplayName("DocumentType.License")]
        License = 3,
        [AppResourceDisplayName("DocumentType.Other")]
        Document = 4
    }
}
