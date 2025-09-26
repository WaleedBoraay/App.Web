namespace App.Core.Security.Permissions
{
    public static class RegistrationPermissions
    {
        // Permission constants
        public const string View = "Registration.View";
        public const string Create = "Registration.Create";
        public const string Edit = "Registration.Edit";
        public const string Submit = "Registration.Submit";
        public const string Review = "Registration.Review";
        public const string Approve = "Registration.Approve";
        public const string Reject = "Registration.Reject";

        // Helper method to generate permission strings dynamically
        public static string For(string action) => $"Registration.{action}";
    }
}