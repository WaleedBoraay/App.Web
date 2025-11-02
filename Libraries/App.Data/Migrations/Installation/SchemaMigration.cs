using App.Core;
using App.Core.Domain.Audit;
using App.Core.Domain.Common;
using App.Core.Domain.Correspondences;
using App.Core.Domain.Directory;
using App.Core.Domain.Institutions;
using App.Core.Domain.Localization;
using App.Core.Domain.Notifications;
using App.Core.Domain.Organization;
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
    [AppMigration("2025/08/29 21:45:00:0000000",
        "Schema migration for initial setup",
        MigrationProcessType.Installation)]
    public class SchemaMigration : ForwardOnlyMigration
    {
        public override void Up()
        {
            // ===== Users =====
            Create.TableFor<AppUser>();
            Create.TableFor<UserLog>();

            // ===== Security (RBAC) =====
            // main entities
            Create.TableFor<Role>();
            Create.TableFor<Permission>();
            Create.TableFor<Privilege>();

            // relations
            Create.TableFor<UserRole>();
            Create.TableFor<RolePermission>();
            Create.TableFor<RolePrivilege>();
            Create.TableFor<UserPermissionOverride>();

            // ===== Organization =====
            // order is important: Department -> Unit -> SubUnit
            Create.TableFor<Sector>();
            Create.TableFor<Department>();
            Create.TableFor<Unit>();

            // ===== Directory =====
            Create.TableFor<BusinessScaleRange>();
            Create.TableFor<EmployeeRange>();
            Create.TableFor<Country>();
            Create.TableFor<StateProvince>();
            Create.TableFor<WorkingLocation>();
            Create.TableFor<CalendarEvent>();

            // ===== Localization =====
            Create.TableFor<Language>();
            Create.TableFor<LocaleStringResource>();
            Create.TableFor<LocalizedProperty>();
            Create.TableFor<LocalizationSettings>();

            // ===== Common =====
            Create.TableFor<Attachment>();
            Create.TableFor<GenericAttribute>();
            Create.TableFor<Template>();

            // ===== Registrations =====
            Create.TableFor<Registration>();
            Create.TableFor<Contact>();
            Create.TableFor<Document>();
            Create.TableFor<RegistrationDocument>();
            Create.TableFor<RegistrationStatusLog>();
            Create.TableFor<WorkflowStep>();

            // ===== Audit =====
            Create.TableFor<AuditTrail>();
            Create.TableFor<AuditTrailTable>();
            Create.TableFor<AuditTrailField>();
            Create.TableFor<UserAuditTrail>();

            // ===== Notifications =====
            Create.TableFor<Notification>();
            Create.TableFor<NotificationLog>();
            Create.TableFor<NotificationReadLog>();

            // ===== Correspondences =====
            Create.TableFor<Correspondence>();

            // ===== Reports =====
            Create.TableFor<Dashboard>();
            Create.TableFor<Report>();

            // ===== Settings =====
            Create.TableFor<Setting>();

            // ===== Institutions =====
            Create.TableFor<Institution>();
            Create.TableFor<InstituteDocument>();
            Create.TableFor<Job>();
            Create.TableFor<Contract>();
            Create.TableFor<Duty>();
            Create.TableFor<Branch>();

            // ===== Composite PKs for junction tables =====
        }
    }
}
