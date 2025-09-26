using System;
using App.Core.Domain.Users;

namespace App.Core.Domain.Audit
{
    public class AuditTrail : BaseEntity
    {
        /// <summary>
        /// Name of the table/entity that was changed
        /// </summary>
        public string EntityName { get; set; }

        /// <summary>
        /// Primary key of the entity that was changed
        /// </summary>
        public int EntityId { get; set; }

        /// <summary>
        /// Field/column that was changed (if applicable)
        /// </summary>
        public string FieldName { get; set; }

        /// <summary>
        /// Old value of the field
        /// </summary>
        public string OldValue { get; set; }

        /// <summary>
        /// New value of the field
        /// </summary>
        public string NewValue { get; set; }

        /// <summary>
        /// UTC timestamp when the change occurred
        /// </summary>
        public DateTime ChangedOnUtc { get; set; }

        /// <summary>
        /// Action performed (Create, Update, Delete)
        /// </summary>
        public int ActionId { get; set; }
        public AuditActionType Action { get; set; }

        /// <summary>
        /// IP address of the client who performed the action
        /// </summary>
        public string ClientIp { get; set; }

        /// <summary>
        /// Optional comment/notes about the change
        /// </summary>
        public string Comment { get; set; }
    }
}
