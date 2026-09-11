using EFCore.Repository.Demo.Entities;
using Microsoft.EntityFrameworkCore;

namespace EFCore.Repository.Demo.Data;


public sealed class DemoDbContext(DbContextOptions<DemoDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Product>().HasQueryFilter(x => !x.IsDeleted).HasOne(x => x.Category).WithMany(x => x.Products).HasForeignKey(x => x.CategoryId);
        b.Entity<Category>().HasQueryFilter(x => !x.IsDeleted);
    }
}
