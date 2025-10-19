namespace App.Core.Security
{
    /// <summary>
    /// Defines all system permission constants.
    /// These values are used as unique keys in the ACL system.
    /// </summary>
    public static class AppPermissions
    {
        // ---------------------------
        // Dashboard / System
        // ---------------------------
        public const string System_Admin = "System.Admin";
        public const string Dashboard_View = "Dashboard.View";

        // ---------------------------
        // Registration
        // ---------------------------
        public const string Registration_Create = "Registration.Create";
        public const string Registration_Read = "Registration.Read";
        public const string Registration_Update = "Registration.Update";
        public const string Registration_Delete = "Registration.Delete";
        public const string Registration_Submit = "Registration.Submit";
        public const string Registration_Validate = "Registration.Validate";
        public const string Registration_Approve = "Registration.Approve";
        public const string Registration_Audit = "Registration.Audit";
        public const string Registration_Archive = "Registration.Archive";
        public const string Registration_ReturnForEdit = "Registration.ReturnForEdit";
        public const string Registration_Reject = "Registration.Reject";

        // ---------------------------
        // Institution
        // ---------------------------
        public const string Institution_Create = "Institution.Create";
        public const string Institution_Read = "Institution.Read";
        public const string Institution_Update = "Institution.Update";
        public const string Institution_Delete = "Institution.Delete";

        // ---------------------------
        // Organization structure
        // ---------------------------
        public const string Organization_Read = "Organization.Read";
        public const string Organization_Manage = "Organization.Manage";

        public const string Department_Read = "Department.Read";
        public const string Department_Manage = "Department.Manage";

        public const string Unit_Read = "Unit.Read";
        public const string Unit_Manage = "Unit.Manage";

        public const string SubUnit_Read = "SubUnit.Read";
        public const string SubUnit_Manage = "SubUnit.Manage";

        // ---------------------------
        // Users
        // ---------------------------
        public const string User_View = "User.View";
        public const string User_Create = "User.Create";
        public const string User_Read = "User.Read";
        public const string User_Update = "User.Update";
        public const string User_Delete = "User.Delete";

        // ---------------------------
        // Roles & Permissions
        // ---------------------------
        public const string Role_Manage = "Role.Manage";
        public const string Permission_Manage = "Permission.Manage";
        public const string UserRoles_Manage = "UserRoles.Manage";
        public const string UserPermissionOverride_Manage = "UserPermissionOverride.Manage";

        // ---------------------------
        // Audit / Logs
        // ---------------------------
        public const string AuditTrail_Read = "AuditTrail.Read";

        // ---------------------------
        // Settings
        // ---------------------------
        public const string Settings_Manage = "Settings.Manage";

        // ---------------------------
        // Localization
        // ---------------------------
        public const string Localization_Language_Manage = "Localization.Language.Manage";
        public const string Localization_Resource_Manage = "Localization.Resource.Manage";

        // ---------------------------
        // Notifications
        // ---------------------------
        public const string Notification_Send = "Notification.Send";

        // ---------------------------
        // Directory (Countries, States)
        // ---------------------------
        public const string Directory_Country_Read = "Directory.Country.Read";
        public const string Directory_Country_Update = "Directory.Country.Update";
        public const string Directory_State_Read = "Directory.State.Read";
        public const string Directory_State_Update = "Directory.State.Update";
    }
}
