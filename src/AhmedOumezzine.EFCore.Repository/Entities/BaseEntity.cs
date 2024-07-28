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

        public void UpdateEntity<T>(T dto) where T : class
        {
            var properties = typeof(T).GetProperties();

            foreach (var property in properties)
            {
                var dtoValue = property.GetValue(dto);
                var entityProperty = this.GetType().GetProperty(property.Name);

                if (entityProperty != null)
                {
                    var entityValue = entityProperty.GetValue(this);

                    if (!object.Equals(entityValue, dtoValue))
                    {
                        entityProperty.SetValue(this, dtoValue);
                    }
                }
            }
        }
    }
}