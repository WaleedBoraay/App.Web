namespace App.Core.Domain.Registrations
{
    /// <summary>
    /// Registration lifecycle per BRD.
    /// </summary>
    public enum RegistrationStatus
    {
        Draft = 0,
        Submitted = 1,
        Approved = 2,
        Rejected = 3,
        ReturnedForEdit = 4,
        Archived = 5,
        FinalSubmission = 6
    }
}
