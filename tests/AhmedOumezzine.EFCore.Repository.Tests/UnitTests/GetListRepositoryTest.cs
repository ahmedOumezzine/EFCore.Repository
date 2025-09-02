using AhmedOumezzine.EFCore.Repository.Repository;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Xunit;

namespace AhmedOumezzine.EFCore.Repository.Tests.UnitTests
{
    public class GetListRepositoryTest
    {
        private TestDbContext GetDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new TestDbContext(options);
        }

        [Fact]
        public async Task GetListAsync_ShouldReturnAllNonDeletedEntities()
        {
            using var context = GetDbContext(nameof(GetListAsync_ShouldReturnAllNonDeletedEntities));
            var repo = new Repository<TestDbContext>(context);

            context.Products.AddRange(new[]
            {
                new Product { Name = "P1" },
                new Product { Name = "P2", IsDeleted = true },
                new Product { Name = "P3" }
            });
            await context.SaveChangesAsync();

            var list = await repo.GetListAsync<Product>();
            Assert.Equal(2, list.Count);
            Assert.DoesNotContain(list, p => p.IsDeleted);
        }

        [Fact]
        public async Task GetListAsync_WithCondition_ShouldReturnFilteredList()
        {
            using var context = GetDbContext(nameof(GetListAsync_WithCondition_ShouldReturnFilteredList));
            var repo = new Repository<TestDbContext>(context);

            context.Products.AddRange(new[]
            {
                new Product { Name = "Alpha" },
                new Product { Name = "Beta" },
                new Product { Name = "AlphaX" }
            });
            await context.SaveChangesAsync();

            Expression<Func<Product, bool>> condition = p => p.Name.StartsWith("Alpha");
            var list = await repo.GetListAsync(condition);
            Assert.Equal(2, list.Count);
            Assert.All(list, p => Assert.StartsWith("Alpha", p.Name));
        }

        [Fact]
        public async Task GetDeletedListAsync_ShouldReturnOnlyDeleted()
        {
            using var context = GetDbContext(nameof(GetDeletedListAsync_ShouldReturnOnlyDeleted));
            var repo = new Repository<TestDbContext>(context);

            context.Products.AddRange(new[]
            {
                new Product { Name = "Active" },
                new Product { Name = "Deleted1", IsDeleted = true },
                new Product { Name = "Deleted2", IsDeleted = true }
            });
            await context.SaveChangesAsync();

            var deleted = await repo.GetDeletedListAsync<Product>();
            Assert.Equal(2, deleted.Count);
            Assert.All(deleted, p => Assert.True(p.IsDeleted));
        }

        [Fact]
        public async Task GetListAsync_WithProjection_ShouldReturnDtoList()
        {
            using var context = GetDbContext(nameof(GetListAsync_WithProjection_ShouldReturnDtoList));
            var repo = new Repository<TestDbContext>(context);

            context.Products.AddRange(new[]
            {
                new Product { Name = "P1" },
                new Product { Name = "P2" }
            });
            await context.SaveChangesAsync();

            var projected = await repo.GetListAsync<Product, string>(p => p.Name);
            Assert.Equal(2, projected.Count);
            Assert.Contains("P1", projected);
            Assert.Contains("P2", projected);
        }

        [Fact]
        public async Task GetDistinctByAsync_ShouldReturnDistinctValues()
        {
            using var context = GetDbContext(nameof(GetDistinctByAsync_ShouldReturnDistinctValues));
            var repo = new Repository<TestDbContext>(context);

            context.Products.AddRange(new[]
            {
                new Product { Name = "A" },
                new Product { Name = "B" },
                new Product { Name = "A" }
            });
            await context.SaveChangesAsync();

            var distinct = await repo.GetDistinctByAsync<Product, string>(p => p.Name);
            Assert.Equal(2, distinct.Count);
            Assert.Contains("A", distinct);
            Assert.Contains("B", distinct);
        }

        [Fact]
        public async Task TryGetListAsync_ShouldReturnSuccessAndItems()
        {
            using var context = GetDbContext(nameof(TryGetListAsync_ShouldReturnSuccessAndItems));
            var repo = new Repository<TestDbContext>(context);

            context.Products.Add(new Product { Name = "Test" });
            await context.SaveChangesAsync();

            var (success, items) = await repo.TryGetListAsync<Product>();
            Assert.True(success);
            Assert.Single(items);
        }

        [Fact]
        public async Task ExistsAnyAndListAsync_ShouldReturnCorrectTuple()
        {
            using var context = GetDbContext(nameof(ExistsAnyAndListAsync_ShouldReturnCorrectTuple));
            var repo = new Repository<TestDbContext>(context);

            context.Products.AddRange(new[]
            {
                new Product { Name = "X" },
                new Product { Name = "Y" }
            });
            await context.SaveChangesAsync();

            var (hasAny, items) = await repo.ExistsAnyAndListAsync<Product>(p => p.Name.StartsWith("X"));
            Assert.True(hasAny);
            Assert.Single(items);
            Assert.Equal("X", items.First().Name);
        }
    }
}