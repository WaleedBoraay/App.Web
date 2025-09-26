using App.Core.Domain.Registrations;
using App.Core.Security;

namespace App.Core.Workflow
{
    public static class RegistrationWorkflowRules
    {
        // Define valid transitions and role gating
        public static bool CanTransition(RegistrationStatus from, RegistrationStatus to)
        {
            // Define valid transitions
            return (from, to) switch
            {
                (RegistrationStatus.Draft, RegistrationStatus.Submitted) => true,
                (RegistrationStatus.Submitted, RegistrationStatus.UnderReview) => true,
                (RegistrationStatus.UnderReview, RegistrationStatus.Approved) => true,
                (RegistrationStatus.UnderReview, RegistrationStatus.Rejected) => true,
                (RegistrationStatus.UnderReview, RegistrationStatus.ReturnedForEdit) => true,
                (RegistrationStatus.ReturnedForEdit, RegistrationStatus.Draft) => true,
                (_, RegistrationStatus.Archived) => true, // Any -> Archived
                _ => false
            };
        }

        public static string[] AllowedRolesFor(RegistrationStatus from, RegistrationStatus to)
        {
            // Define role gating for transitions
            return (from, to) switch
            {
                (RegistrationStatus.Draft, RegistrationStatus.Submitted) => new[] { SystemRoles.Maker, SystemRoles.Checker, SystemRoles.Admin },
                (RegistrationStatus.Submitted, RegistrationStatus.UnderReview) => new[] { SystemRoles.Checker, SystemRoles.Admin },
                (RegistrationStatus.UnderReview, RegistrationStatus.Approved) => new[] { SystemRoles.Regulator, SystemRoles.Admin },
                (RegistrationStatus.UnderReview, RegistrationStatus.Rejected) => new[] { SystemRoles.Regulator, SystemRoles.Admin },
                (RegistrationStatus.UnderReview, RegistrationStatus.ReturnedForEdit) => new[] { SystemRoles.Checker, SystemRoles.Regulator, SystemRoles.Admin },
                (RegistrationStatus.ReturnedForEdit, RegistrationStatus.Draft) => new[] { SystemRoles.Maker, SystemRoles.Checker, SystemRoles.Admin },
                (_, RegistrationStatus.Archived) => new[] { SystemRoles.Admin }, // Any -> Archived
                _ => Array.Empty<string>() // No roles allowed for invalid transitions
            };
        }
    }
}