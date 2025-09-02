using AhmedOumezzine.EFCore.Repository.Repository;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AhmedOumezzine.EFCore.Repository.Tests.UnitTests
{
    public class GetRepositoryTest
    {
        private TestDbContext GetDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new TestDbContext(options);
        }

        [Fact]
        public async Task GetAsync_ByCondition_ReturnsCorrectEntity()
        {
            using var context = GetDbContext(nameof(GetAsync_ByCondition_ReturnsCorrectEntity));
            var repo = new Repository<TestDbContext>(context);

            context.Products.AddRange(new[]
            {
                new Product { Name = "A1" },
                new Product { Name = "B1" },
                new Product { Name = "Deleted", IsDeleted = true }
            });
            await context.SaveChangesAsync();

            var entity = await repo.GetAsync<Product>(p => p.Name == "A1");

            Assert.NotNull(entity);
            Assert.Equal("A1", entity.Name);
        }

        [Fact]
        public async Task GetAsync_SoftDeletedEntity_ReturnsNull()
        {
            using var context = GetDbContext(nameof(GetAsync_SoftDeletedEntity_ReturnsNull));
            var repo = new Repository<TestDbContext>(context);

            context.Products.Add(new Product { Name = "Deleted", IsDeleted = true });
            await context.SaveChangesAsync();

            var entity = await repo.GetAsync<Product>(p => p.Name == "Deleted");
            Assert.Null(entity);
        }

        [Fact]
        public async Task GetOnlyAsync_ReturnsSingleProperty()
        {
            using var context = GetDbContext(nameof(GetOnlyAsync_ReturnsSingleProperty));
            var repo = new Repository<TestDbContext>(context);

            context.Products.Add(new Product { Name = "TestName" });
            await context.SaveChangesAsync();

            var name = await repo.GetOnlyAsync<Product, string>(p => p.Name == "TestName", p => p.Name);
            Assert.Equal("TestName", name);
        }

        [Fact]
        public async Task TryGetAsync_ReturnsSuccessAndEntity()
        {
            using var context = GetDbContext(nameof(TryGetAsync_ReturnsSuccessAndEntity));
            var repo = new Repository<TestDbContext>(context);

            context.Products.Add(new Product { Name = "Test" });
            await context.SaveChangesAsync();

            var (success, entity) = await repo.TryGetAsync<Product>(p => p.Name == "Test");
            Assert.True(success);
            Assert.NotNull(entity);
            Assert.Equal("Test", entity.Name);
        }

        [Fact]
        public async Task GetAsync_Projection_ReturnsDto()
        {
            using var context = GetDbContext(nameof(GetAsync_Projection_ReturnsDto));
            var repo = new Repository<TestDbContext>(context);

            context.Products.Add(new Product { Name = "P1" });
            await context.SaveChangesAsync();

            var dto = await repo.GetAsync<Product, string>(p => p.Name == "P1", p => p.Name);
            Assert.Equal("P1", dto);
        }

        [Fact]
        public async Task GetFirstOrThrowAsync_ThrowsIfNotFound()
        {
            using var context = GetDbContext(nameof(GetFirstOrThrowAsync_ThrowsIfNotFound));
            var repo = new Repository<TestDbContext>(context);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                repo.GetFirstOrThrowAsync<Product>(p => p.Name == "NonExistent", "Not found"));
        }

        [Fact]
        public async Task ExistsAndFetchAsync_ReturnsCorrectTuple()
        {
            using var context = GetDbContext(nameof(ExistsAndFetchAsync_ReturnsCorrectTuple));
            var repo = new Repository<TestDbContext>(context);

            context.Products.Add(new Product { Name = "X" });
            await context.SaveChangesAsync();

            var (exists, entity) = await repo.ExistsAndFetchAsync<Product>(p => p.Name == "X");
            Assert.True(exists);
            Assert.NotNull(entity);
            Assert.Equal("X", entity.Name);
        }
    }
}