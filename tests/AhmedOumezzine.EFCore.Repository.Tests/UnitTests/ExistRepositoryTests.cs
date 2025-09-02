using AhmedOumezzine.EFCore.Repository.Repository;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AhmedOumezzine.EFCore.Repository.Tests.UnitTests
{
    public class RepositoryExistsTests
    {
        private TestDbContext GetDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new TestDbContext(options);
        }

        [Fact]
        public async Task ExistsByIdAsync_ShouldReturnTrue_WhenEntityExists()
        {
            using var context = GetDbContext(nameof(ExistsByIdAsync_ShouldReturnTrue_WhenEntityExists));
            var repo = new Repository<TestDbContext>(context);

            var product = new Product { Name = "TestProduct" };
            context.Products.Add(product);
            await context.SaveChangesAsync();

            var exists = await repo.ExistsByIdAsync<Product>(product.Id);
            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsByIdAsync_ShouldReturnFalse_WhenEntityDoesNotExist()
        {
            using var context = GetDbContext(nameof(ExistsByIdAsync_ShouldReturnFalse_WhenEntityDoesNotExist));
            var repo = new Repository<TestDbContext>(context);

            var exists = await repo.ExistsByIdAsync<Product>(Guid.NewGuid());
            Assert.False(exists);
        }

        [Fact]
        public async Task ExistsAsync_ShouldReturnTrue_WhenAnyEntityExists()
        {
            using var context = GetDbContext(nameof(ExistsAsync_ShouldReturnTrue_WhenAnyEntityExists));
            var repo = new Repository<TestDbContext>(context);

            context.Products.Add(new Product { Name = "Prod1" });
            await context.SaveChangesAsync();

            var exists = await repo.ExistsAsync<Product>();
            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsAsync_WithCondition_ShouldReturnTrue_WhenConditionMatches()
        {
            using var context = GetDbContext(nameof(ExistsAsync_WithCondition_ShouldReturnTrue_WhenConditionMatches));
            var repo = new Repository<TestDbContext>(context);

            context.Products.AddRange(
                new Product { Name = "Apple" },
                new Product { Name = "Banana" });
            await context.SaveChangesAsync();

            var exists = await repo.ExistsAsync<Product>(p => p.Name == "Apple");
            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsByCompositeKeyAsync_ShouldReturnTrue_WhenEntityExists()
        {
            using var context = GetDbContext(nameof(ExistsByCompositeKeyAsync_ShouldReturnTrue_WhenEntityExists));
            var repo = new Repository<TestDbContext>(context);

            var product = new Product { Name = "CompositeTest" };
            context.Products.Add(product);
            await context.SaveChangesAsync();

            var exists = await repo.ExistsByCompositeKeyAsync<Product>(new object[] { product.Id });
            Assert.True(exists);
        }

        [Fact]
        public async Task CountAsync_ShouldReturnCorrectNumber()
        {
            using var context = GetDbContext(nameof(CountAsync_ShouldReturnCorrectNumber));
            var repo = new Repository<TestDbContext>(context);

            context.Products.AddRange(
                new Product { Name = "A" },
                new Product { Name = "B", IsDeleted = true },
                new Product { Name = "C" });
            await context.SaveChangesAsync();

            var count = await repo.CountAsync<Product>();
            Assert.Equal(2, count); // Seules les entités non supprimées comptent
        }
    }
}