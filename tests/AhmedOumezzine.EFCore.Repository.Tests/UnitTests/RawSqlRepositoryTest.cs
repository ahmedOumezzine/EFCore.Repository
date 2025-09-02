using AhmedOumezzine.EFCore.Repository.Repository;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AhmedOumezzine.EFCore.Repository.Tests.UnitTests
{
    public class RawSqlRepositoryTest
    {
        private TestDbContext GetDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseSqlite("Filename=:memory:")  // SQLite in-memory
                .Options;

            var context = new TestDbContext(options);

            // Ouvrir la connexion et créer les tables
            context.Database.OpenConnection();
            context.Database.EnsureCreated();

            return context;
        }

        [Fact]
        public async Task ExecuteSqlCommandAsync_InsertsRowSuccessfully()
        {
            using var context = GetDbContext(nameof(ExecuteSqlCommandAsync_InsertsRowSuccessfully));
            var repo = new Repository<TestDbContext>(context);

            // Générer un nouveau GUID pour l'ID
            var newId = Guid.NewGuid();

            // Fournir l'ID explicitement dans le SQL
            var sql = $"INSERT INTO Products (Id, Name, Description, IsActive, CreatedOnUtc, IsDeleted) " +
                      $"VALUES ('{newId}', 'RawTest', '', 1, '{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}', 0)";

            var affected = await repo.ExecuteSqlCommandAsync(sql);

            Assert.Equal(1, affected);

            var inserted = await context.Products.FirstOrDefaultAsync();
            Assert.NotNull(inserted);
            Assert.Equal(newId, inserted.Id);
            Assert.Equal("RawTest", inserted.Name);
        }

        [Fact]
        public async Task GetFromRawSqlAsync_ReturnsEntities()
        {
            using var context = GetDbContext(nameof(GetFromRawSqlAsync_ReturnsEntities));
            var repo = new Repository<TestDbContext>(context);

            context.Products.Add(new Product { Name = "P1" });
            await context.SaveChangesAsync();

            var sql = "SELECT * FROM Products";
            var list = await repo.GetFromRawSqlAsync<Product>(sql);

            Assert.Single(list);
            Assert.Equal("P1", list[0].Name);
        }

        [Fact]
        public async Task ExecuteScalarAsync_ReturnsCorrectValue()
        {
            using var context = GetDbContext(nameof(ExecuteScalarAsync_ReturnsCorrectValue));
            var repo = new Repository<TestDbContext>(context);

            context.Products.AddRange(
                new Product { Name = "A" },
                new Product { Name = "B" }
            );
            await context.SaveChangesAsync();

            var sql = "SELECT COUNT(*) FROM Products";
            var count = await repo.ExecuteScalarAsync<int>(sql);

            Assert.Equal(2, count);
        }

        [Fact]
        public async Task GetSingleFromSqlAsync_ReturnsFirstEntity()
        {
            using var context = GetDbContext(nameof(GetSingleFromSqlAsync_ReturnsFirstEntity));
            var repo = new Repository<TestDbContext>(context);

            var first = new Product { Name = "First", CreatedOnUtc = DateTime.UtcNow.AddMinutes(-1) };
            var second = new Product { Name = "Second", CreatedOnUtc = DateTime.UtcNow };
            context.Products.AddRange(first, second);
            await context.SaveChangesAsync();

            // Trier explicitement pour récupérer le premier inséré
            var sql = "SELECT * FROM Products ORDER BY CreatedOnUtc ASC";
            var entity = await repo.GetSingleFromSqlAsync<Product>(sql);

            Assert.NotNull(entity);
            Assert.Equal("First", entity.Name);
        }

        [Fact]
        public async Task ExistsBySqlAsync_ReturnsTrueIfExists()
        {
            using var context = GetDbContext(nameof(ExistsBySqlAsync_ReturnsTrueIfExists));
            var repo = new Repository<TestDbContext>(context);

            context.Products.Add(new Product { Name = "Exists" });
            await context.SaveChangesAsync();

            var sql = "SELECT COUNT(*) FROM Products WHERE Name = 'Exists'";
            var exists = await repo.ExistsBySqlAsync(sql);

            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsBySqlAsync_ReturnsFalseIfNotExists()
        {
            using var context = GetDbContext(nameof(ExistsBySqlAsync_ReturnsFalseIfNotExists));
            var repo = new Repository<TestDbContext>(context);

            var sql = "SELECT COUNT(*) FROM Products WHERE Name = 'NonExistent'";
            var exists = await repo.ExistsBySqlAsync(sql);

            Assert.False(exists);
        }
    }
}