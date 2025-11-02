using App.Core;
using App.Core.Domain.Audit;
using App.Core.Domain.Common;
using App.Core.Domain.Correspondences;
using App.Core.Domain.Directory;
using App.Core.Domain.Institutions;
using App.Core.Domain.Localization;
using App.Core.Domain.Notifications;
using App.Core.Domain.Ref;
using App.Core.Domain.Registrations;
using App.Core.Domain.Reports;
using App.Core.Domain.Security;
using App.Core.Domain.Settings;
using App.Core.Domain.Users;
using App.Data.Configuration;
using App.Data.MigratorServices;
using App.Data.MigratorServices.Extensions;
using FluentMigrator;
using System.Reflection;

namespace App.Data.Migrations.Installation
{
    [AppMigration("2025/09/18 21:45:00:0000001",
        "Schema migration for initial setup",
        MigrationProcessType.Update)]
    public class SchemaMigrationFIDocumentUpdate : MigrationBase
    {
        public override void Down()
        {
        }

        public override void Up()
        {
            Create.TableFor<Document>();
        }
    }
}
