namespace AhmedOumezzine.EFCore.Repository.Tests.Models
{
    public class Product : AhmedOumezzine.EFCore.Repository.Entities.BaseEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
    }
}