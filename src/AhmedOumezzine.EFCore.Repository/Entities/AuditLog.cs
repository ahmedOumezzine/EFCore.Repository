
using System;

namespace AhmedOumezzine.EFCore.Repository.Entities
{

    namespace AhmedOumezzine.EFCore.Repository.Entities
    {
        /// <summary>
        /// Represents an audit log entry for tracking changes in the system.
        /// </summary>
        public class AuditLog : BaseEntity
        {
            /// <summary>
            /// The action performed (e.g., INSERT, UPDATE, DELETE).
            /// </summary>
            public string Action { get; set; } = string.Empty;

            /// <summary>
            /// The name of the entity type affected (e.g., "Customer").
            /// </summary>
            public string EntityName { get; set; } = string.Empty;

            /// <summary>
            /// The ID of the entity (as string to be generic).
            /// </summary>
            public string EntityId { get; set; } = string.Empty;

            /// <summary>
            /// The username of the user who performed the action.
            /// </summary>
            public string UserName { get; set; } = string.Empty;

            /// <summary>
            /// Additional details (optional, e.g., JSON snapshot).
            /// </summary>
            public string Details { get; set; } = string.Empty;

            /// <summary>
            /// UTC timestamp of when the action was logged.
            /// </summary>
            public DateTime CreatedOnUtc { get; set; }  
        }
    }

}
