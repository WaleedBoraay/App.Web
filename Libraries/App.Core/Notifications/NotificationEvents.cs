using System.Collections.Generic;

namespace App.Core.Notifications
{
    public static class NotificationEvents
    {
        // Event names
        public const string RegistrationSubmitted = "RegistrationSubmitted";
        public const string RegistrationApproved = "RegistrationApproved";
        public const string RegistrationRejected = "RegistrationRejected";
        public const string ReturnedForEdit = "ReturnedForEdit";
        public const string NewAssignment = "NewAssignment";

        // Method to get recipients based on event and registration model
        public static IEnumerable<Recipient> GetRecipients(string evt)
        {
            var recipients = new List<Recipient>();

            switch (evt)
            {
                case RegistrationSubmitted:
                    recipients.Add(new Recipient { UserId = 1, Email = "checker@example.com" }); // Example recipient
                    break;

                case RegistrationApproved:
                    recipients.Add(new Recipient { UserId = 2, Email = "admin@example.com" }); // Example recipient
                    break;

                case RegistrationRejected:
                    recipients.Add(new Recipient { UserId = 3, Email = "maker@example.com" }); // Example recipient
                    break;

                case ReturnedForEdit:
                    recipients.Add(new Recipient { UserId = 4, Email = "maker@example.com" }); // Example recipient
                    break;

                case NewAssignment:
                    recipients.Add(new Recipient { UserId = 5, Email = "assigned@example.com" }); // Example recipient
                    break;

                default:
                    break;
            }

            return recipients;
        }
    }

    // Recipient class
    public class Recipient
    {
        public int UserId { get; set; }
        public string? Email { get; set; }
    }
}