namespace App.Core.Domain.Registrations
{
    /// <summary>
    /// Registration lifecycle per BRD.
    /// </summary>
    public enum RegistrationStatus
    {
        Draft = 0,
        Submitted = 1,
        UnderReview = 2,
        Approved = 3,
        Rejected = 4,
        ReturnedForEdit = 5,
        Archived = 6,
        FinalSubmission = 7
    }
}
