using AhmedOumezzine.EFCore.Repository.Entities;
using AhmedOumezzine.EFCore.Repository.Repository;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
public sealed class Product : BaseEntity { public string Name { get; set; } = ""; }
public sealed class ProductContext : DbContext { public ProductContext(DbContextOptions<ProductContext> o):base(o){} public DbSet<Product> Products=>Set<Product>(); }
public static class Program { public static async Task<int> Main() { var connection = new SqliteConnection("DataSource=:memory:"); connection.Open(); var options = new DbContextOptionsBuilder<ProductContext>().UseSqlite(connection).Options; await using var db = new ProductContext(options); await db.Database.EnsureCreatedAsync(); var repo = new Repository<ProductContext>(db); var product = new Product { Name = "Consumer" }; await repo.InsertAsync(product); var loaded = await repo.GetByIdAsync<Product>(product.Id); if (loaded?.Name != "Consumer") return 1; loaded.Name = "Updated"; await repo.UpdateAsync(loaded); await repo.DeleteAsync(loaded); if (!loaded.IsDeleted) return 2; await repo.RestoreAsync(loaded); if (loaded.IsDeleted) return 3; Console.WriteLine("Consumer CRUD + soft-delete PASS"); return 0; } }
