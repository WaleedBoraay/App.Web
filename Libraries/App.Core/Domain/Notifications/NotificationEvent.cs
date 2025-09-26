namespace App.Core.Domain.Notifications
{
    /// <summary>
    /// Defines system notification events as per BRD.
    /// </summary>
    public enum NotificationEvent
    {
        // Workflow - Draft/Submission
        RegistrationSubmitted = 1,

        // Validation
        RegistrationValidated = 2,

        // Approval
        RegistrationApproved = 3,
        RegistrationRejected = 4,
        RegistrationReturnedForEdit = 5,

        // Audit
        RegistrationAudited = 6,

        // Final
        RegistrationFinalSubmission = 7,
        RegistrationArchived = 8,

        // Assignment
        NewAssignment = 9,
        UserCreated = 10,
        UserDeleted = 11,
        RoleRevoked = 12,
        UserUpdated = 13,
        RoleAssigned = 14,
        PermissionGranted = 15,
        InstitutionCreated = 16
    }
}
