using AhmedOumezzine.EFCore.Repository.Repository;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AhmedOumezzine.EFCore.Repository.Tests.UnitTests
{
    public class DeleteRepositoryTests
    {
        private TestDbContext GetDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new TestDbContext(options);
        }

        [Fact]
        public async Task Remove_ShouldMarkEntityAsDeleted()
        {
            using var context = GetDbContext(nameof(Remove_ShouldMarkEntityAsDeleted));
            var repo = new Repository<TestDbContext>(context);

            var product = new Product { Name = "TestProduct" };
            await repo.InsertAsync(product);

            repo.Remove(product);
            await context.SaveChangesAsync();

            var dbProduct = await context.Products.FindAsync(product.Id);
            Assert.True(dbProduct.IsDeleted);
            Assert.NotNull(dbProduct.DeletedOnUtc);
        }

        [Fact]
        public async Task DeleteAsync_ShouldSoftDeleteEntity()
        {
            using var context = GetDbContext(nameof(DeleteAsync_ShouldSoftDeleteEntity));
            var repo = new Repository<TestDbContext>(context);

            var product = new Product { Name = "TestProduct" };
            await repo.InsertAsync(product);

            await repo.DeleteAsync(product);

            var dbProduct = await context.Products.FindAsync(product.Id);
            Assert.True(dbProduct.IsDeleted);
            Assert.NotNull(dbProduct.DeletedOnUtc);
        }

        [Fact]
        public async Task TryDeleteAsync_ShouldReturnTrue_OnSuccess()
        {
            using var context = GetDbContext(nameof(TryDeleteAsync_ShouldReturnTrue_OnSuccess));
            var repo = new Repository<TestDbContext>(context);

            var product = new Product { Name = "TestProduct" };
            await repo.InsertAsync(product);

            var result = await repo.TryDeleteAsync(product);

            Assert.True(result);
            var dbProduct = await context.Products.FindAsync(product.Id);
            Assert.True(dbProduct.IsDeleted);
        }

        [Fact]
        public async Task RestoreAsync_ShouldRestoreSoftDeletedEntity()
        {
            using var context = GetDbContext(nameof(RestoreAsync_ShouldRestoreSoftDeletedEntity));
            var repo = new Repository<TestDbContext>(context);

            var product = new Product { Name = "TestProduct", IsDeleted = true, DeletedOnUtc = DateTime.UtcNow };
            context.Products.Add(product);
            await context.SaveChangesAsync();

            await repo.RestoreAsync(product);

            var dbProduct = await context.Products.FindAsync(product.Id);
            Assert.False(dbProduct.IsDeleted);
            Assert.Null(dbProduct.DeletedOnUtc);
        }

        [Fact]
        public async Task DeleteRangeByConditionAsync_ShouldSoftDeleteMatchingEntities()
        {
            using var context = GetDbContext(nameof(DeleteRangeByConditionAsync_ShouldSoftDeleteMatchingEntities));
            var repo = new Repository<TestDbContext>(context);

            var products = new List<Product>
            {
                new Product { Name = "OldProduct", CreatedOnUtc = DateTime.UtcNow.AddYears(-3) },
                new Product { Name = "NewProduct", CreatedOnUtc = DateTime.UtcNow }
            };
            context.Products.AddRange(products);
            await context.SaveChangesAsync();

            int deletedCount = await repo.DeleteRangeByConditionAsync<Product>(
                p => p.CreatedOnUtc < DateTime.UtcNow.AddYears(-1));

            Assert.Equal(1, deletedCount);
            Assert.True(context.Products.First(p => p.Name == "OldProduct").IsDeleted);
            Assert.False(context.Products.First(p => p.Name == "NewProduct").IsDeleted);
        }
    }
}