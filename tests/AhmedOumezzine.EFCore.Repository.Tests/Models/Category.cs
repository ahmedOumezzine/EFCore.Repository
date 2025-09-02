namespace AhmedOumezzine.EFCore.Repository.Tests.Models
{
    public class Category : AhmedOumezzine.EFCore.Repository.Entities.BaseEntity
    {
        public string Name { get; set; }
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}