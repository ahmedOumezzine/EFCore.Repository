using System.ComponentModel.DataAnnotations;

namespace AhmedOumezzine.EFCore.Repository.Entities
{
/// <summary>Base entity with identity and audit fields.</summary>
public abstract class BaseEntity
    {
        /// <summary>Primary key.</summary>
        [Key]
        public Guid Id { get; set; }
        /// <summary>Creation timestamp in UTC.</summary>
        [Required]
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>Last modification timestamp in UTC.</summary>
        public DateTime? LastModifiedOnUtc { get; set; }
        /// <summary>Whether the entity is soft deleted.</summary>
        public bool IsDeleted { get; set; }
        /// <summary>Soft deletion timestamp in UTC.</summary>
        public DateTime? DeletedOnUtc { get; set; }
    }
}
