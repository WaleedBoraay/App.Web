namespace App.Core.Domain.Registrations
{
    /// <summary>
    /// Defines available actions for a registration.
    /// </summary>
    public enum RegistrationAction
    {
        Submit,
        Validate,
        Approve,
        Reject,
        ReturnForEdit,
        Audit,
        Archive,
        FinalSubmission
    }
}
