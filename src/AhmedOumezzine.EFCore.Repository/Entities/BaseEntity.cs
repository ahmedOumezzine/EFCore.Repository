using System.ComponentModel.DataAnnotations;

namespace AhmedOumezzine.EFCore.Repository.Entities
{
    public abstract class BaseEntity
    {
        [Key]
        public Guid Id { get; set; } 
         

        [Required]
        public DateTime CreatedOnUtc { get; set; } 
        public DateTime? LastModifiedOnUtc { get; set; } 
        public bool IsDeleted { get; set; }
        public DateTime? DeletedOnUtc { get; set; }
    }
}