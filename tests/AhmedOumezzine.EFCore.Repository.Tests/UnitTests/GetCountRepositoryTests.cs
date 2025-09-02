using AhmedOumezzine.EFCore.Repository.Repository;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Xunit;

namespace AhmedOumezzine.EFCore.Repository.Tests.UnitTests
{
    public class GetCountRepositoryTests
    {
        private TestDbContext GetDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new TestDbContext(options);
        }

        [Fact]
        public async Task GetCountAsync_ShouldReturnCorrectCount()
        {
            using var context = GetDbContext(nameof(GetCountAsync_ShouldReturnCorrectCount));
            var repo = new Repository<TestDbContext>(context);

            context.Products.AddRange(new[]
            {
                new Product { Name = "P1" },
                new Product { Name = "P2" },
                new Product { Name = "P3", IsDeleted = true }
            });
            await context.SaveChangesAsync();

            var count = await repo.GetCountAsync<Product>();
            Assert.Equal(2, count);
        }

        [Fact]
        public async Task GetCountAsync_WithCondition_ShouldReturnFilteredCount()
        {
            using var context = GetDbContext(nameof(GetCountAsync_WithCondition_ShouldReturnFilteredCount));
            var repo = new Repository<TestDbContext>(context);

            context.Products.AddRange(new[]
            {
                new Product { Name = "A" },
                new Product { Name = "B" },
                new Product { Name = "C" }
            });
            await context.SaveChangesAsync();

            Expression<Func<Product, bool>> condition = p => p.Name.StartsWith("A");
            var count = await repo.GetCountAsync(condition);
            Assert.Equal(1, count);
        }

        [Fact]
        public async Task HasAnyAsync_ShouldReturnTrue_WhenEntityExists()
        {
            using var context = GetDbContext(nameof(HasAnyAsync_ShouldReturnTrue_WhenEntityExists));
            var repo = new Repository<TestDbContext>(context);

            context.Products.Add(new Product { Name = "Exists" });
            await context.SaveChangesAsync();

            var result = await repo.HasAnyAsync<Product>();
            Assert.True(result);
        }

        [Fact]
        public async Task HasAnyAsync_WithCondition_ShouldReturnCorrectResult()
        {
            using var context = GetDbContext(nameof(HasAnyAsync_WithCondition_ShouldReturnCorrectResult));
            var repo = new Repository<TestDbContext>(context);

            context.Products.AddRange(new[]
            {
                new Product { Name = "Alpha" },
                new Product { Name = "Beta" }
            });
            await context.SaveChangesAsync();

            Expression<Func<Product, bool>> condition = p => p.Name.StartsWith("B");
            var result = await repo.HasAnyAsync(condition);
            Assert.True(result);
        }

        [Fact]
        public async Task CountDeletedAsync_ShouldReturnOnlyDeletedEntities()
        {
            using var context = GetDbContext(nameof(CountDeletedAsync_ShouldReturnOnlyDeletedEntities));
            var repo = new Repository<TestDbContext>(context);

            context.Products.AddRange(new[]
            {
                new Product { Name = "Active" },
                new Product { Name = "Deleted", IsDeleted = true }
            });
            await context.SaveChangesAsync();

            var count = await repo.CountDeletedAsync<Product>();
            Assert.Equal(1, count);
        }

        [Fact]
        public async Task CountByStatusAsync_ShouldReturnGroupedCounts()
        {
            using var context = GetDbContext(nameof(CountByStatusAsync_ShouldReturnGroupedCounts));
            var repo = new Repository<TestDbContext>(context);

            context.Products.AddRange(new[]
            {
                new Product { Name = "Active", IsActive = true },
                new Product { Name = "Inactive", IsActive = false },
                new Product { Name = "AlsoActive", IsActive = true }
            });
            await context.SaveChangesAsync();

            var result = await repo.CountByStatusAsync<Product>(p => p.IsActive);
            Assert.Equal(2, result[true]);
            Assert.Equal(1, result[false]);
        }

        [Fact]
        public async Task CountByDateRangeAsync_ShouldReturnCorrectCount()
        {
            using var context = GetDbContext(nameof(CountByDateRangeAsync_ShouldReturnCorrectCount));
            var repo = new Repository<TestDbContext>(context);

            var now = DateTime.UtcNow;
            context.Products.AddRange(new[]
            {
                new Product { Name = "Old", CreatedOnUtc = now.AddDays(-10) },
                new Product { Name = "Recent", CreatedOnUtc = now.AddDays(-1) }
            });
            await context.SaveChangesAsync();

            var count = await repo.CountByDateRangeAsync<Product>(now.AddDays(-5), now);
            Assert.Equal(1, count);
        }

        [Fact]
        public async Task CountSoftDeletedAsync_ShouldReturnCorrectCount()
        {
            using var context = GetDbContext(nameof(CountSoftDeletedAsync_ShouldReturnCorrectCount));
            var repo = new Repository<TestDbContext>(context);

            context.Products.AddRange(new[]
            {
                new Product { Name = "Active" },
                new Product { Name = "Deleted", IsDeleted = true }
            });
            await context.SaveChangesAsync();

            var count = await repo.CountSoftDeletedAsync<Product>();
            Assert.Equal(1, count);
        }
    }
}