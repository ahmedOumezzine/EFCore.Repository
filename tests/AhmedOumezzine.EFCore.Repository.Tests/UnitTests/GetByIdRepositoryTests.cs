using AhmedOumezzine.EFCore.Repository.Repository;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AhmedOumezzine.EFCore.Repository.Tests.UnitTests
{
    public class GetByIdRepositoryTests
    {
        private TestDbContext GetDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new TestDbContext(options);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnEntity_WhenExists()
        {
            using var context = GetDbContext(nameof(GetByIdAsync_ShouldReturnEntity_WhenExists));
            var repo = new Repository<TestDbContext>(context);

            var product = new Product { Name = "TestProduct" };
            context.Products.Add(product);
            await context.SaveChangesAsync();

            var result = await repo.GetByIdAsync<Product>(product.Id);
            Assert.NotNull(result);
            Assert.Equal(product.Name, result.Name);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
        {
            using var context = GetDbContext(nameof(GetByIdAsync_ShouldReturnNull_WhenNotExists));
            var repo = new Repository<TestDbContext>(context);

            var result = await repo.GetByIdAsync<Product>(Guid.NewGuid());
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_WithIncludes_ShouldReturnEntity()
        {
            using var context = GetDbContext(nameof(GetByIdAsync_WithIncludes_ShouldReturnEntity));
            var repo = new Repository<TestDbContext>(context);

            var product = new Product { Name = "ProductWithInclude" };
            context.Products.Add(product);
            await context.SaveChangesAsync();

            var result = await repo.GetByIdAsync<Product>(
                product.Id
            );

            Assert.NotNull(result);
            Assert.Equal(product.Name, result.Name);
        }

        [Fact]
        public async Task GetByIdAsync_AsNoTracking_ShouldReturnEntity()
        {
            using var context = GetDbContext(nameof(GetByIdAsync_AsNoTracking_ShouldReturnEntity));
            var repo = new Repository<TestDbContext>(context);

            var product = new Product { Name = "NoTrackingProduct" };
            context.Products.Add(product);
            await context.SaveChangesAsync();

            var result = await repo.GetByIdAsync<Product>(product.Id, asNoTracking: true);
            Assert.NotNull(result);
            Assert.Equal(product.Name, result.Name);
        }

        [Fact]
        public async Task GetByIdsAsync_ShouldReturnAllEntities()
        {
            using var context = GetDbContext(nameof(GetByIdsAsync_ShouldReturnAllEntities));
            var repo = new Repository<TestDbContext>(context);

            var products = new List<Product>
            {
                new Product { Name = "P1" },
                new Product { Name = "P2" },
                new Product { Name = "P3" }
            };
            context.Products.AddRange(products);
            await context.SaveChangesAsync();

            var ids = products.Select(p => (object)p.Id).ToList();
            var results = await repo.GetByIdsAsync<Product>(ids);

            Assert.Equal(3, results.Count);
        }

        [Fact]
        public async Task GetOnlyAsync_ShouldReturnPropertyValue()
        {
            using var context = GetDbContext(nameof(GetOnlyAsync_ShouldReturnPropertyValue));
            var repo = new Repository<TestDbContext>(context);

            var product = new Product { Name = "SinglePropertyProduct" };
            context.Products.Add(product);
            await context.SaveChangesAsync();

            var name = await repo.GetOnlyAsync<Product, string>(
                product.Id,
                p => p.Name
            );

            Assert.Equal(product.Name, name);
        }

        [Fact]
        public async Task TryGetByIdAsync_ShouldReturnTrue_WhenEntityExists()
        {
            using var context = GetDbContext(nameof(TryGetByIdAsync_ShouldReturnTrue_WhenEntityExists));
            var repo = new Repository<TestDbContext>(context);

            var product = new Product { Name = "SafeAccessProduct" };
            context.Products.Add(product);
            await context.SaveChangesAsync();

            var (success, entity) = await repo.TryGetByIdAsync<Product>(product.Id);

            Assert.True(success);
            Assert.NotNull(entity);
            Assert.Equal(product.Name, entity.Name);
        }

        [Fact]
        public async Task TryGetByIdAsync_ShouldReturnFalse_WhenEntityNotExists()
        {
            using var context = GetDbContext(nameof(TryGetByIdAsync_ShouldReturnFalse_WhenEntityNotExists));
            var repo = new Repository<TestDbContext>(context);

            var (success, entity) = await repo.TryGetByIdAsync<Product>(Guid.NewGuid());

            Assert.False(success);
            Assert.Null(entity);
        }
    }
}