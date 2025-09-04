
using AhmedOumezzine.EFCore.Repository.Entities;
using AhmedOumezzine.EFCore.Repository.Repository;
using Castle.Core.Resource;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AhmedOumezzine.EFCore.Repository.Tests.UnitTests
{
    public class GetCountRepositoryTests
    {
        private TestDbContext GetDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            return new TestDbContext(options);
        }

        #region GetCountAsync
        [Fact]
        public async Task GetCountAsync_ShouldReturnCorrectCount()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetCountAsync_ShouldReturnCorrectCount));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddRangeAsync(
                new Product { Name = "P1" },
                new Product { Name = "P2" },
                new Product { Name = "P3", IsDeleted = true });
            await context.SaveChangesAsync();

            // Act
            var count = await repo.GetCountAsync<Product>();

            // Assert
            Assert.Equal(2, count);
        }

        [Fact]
        public async Task GetCountAsync_ShouldReturnZero_WhenNoEntitiesExist()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetCountAsync_ShouldReturnZero_WhenNoEntitiesExist));
            var repo = new Repository<TestDbContext>(context);

            // Act
            var count = await repo.GetCountAsync<Product>();

            // Assert
            Assert.Equal(0, count);
        }

        [Fact]
        public async Task GetCountAsync_WithCondition_ShouldReturnFilteredCount()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetCountAsync_WithCondition_ShouldReturnFilteredCount));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddRangeAsync(
                new Product { Name = "Apple" },
                new Product { Name = "Banana" },
                new Product { Name = "Cherry" });
            await context.SaveChangesAsync();

            // Act
            var count = await repo.GetCountAsync<Product>(p => p.Name.StartsWith("A"));

            // Assert
            Assert.Equal(1, count);
        }

        [Fact]
        public async Task GetCountAsync_WithCondition_ShouldReturnAll_WhenConditionIsNull()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetCountAsync_WithCondition_ShouldReturnAll_WhenConditionIsNull));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddRangeAsync(
                new Product { Name = "P1" },
                new Product { Name = "P2" });
            await context.SaveChangesAsync();

            // Act
            var count = await repo.GetCountAsync<Product>((Expression<Func<Product, bool>>)null);

            // Assert
            Assert.Equal(2, count);
        }

        [Fact]
        public async Task GetCountAsync_WithMultipleConditions_ShouldReturnFilteredCount()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetCountAsync_WithMultipleConditions_ShouldReturnFilteredCount));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddRangeAsync(
                new Product { Name = "Apple", IsActive = true },
                new Product { Name = "Banana", IsActive = true },
                new Product { Name = "Cherry", IsActive = false });
            await context.SaveChangesAsync();
            var conditions = new List<Expression<Func<Product, bool>>>
            {
                p => p.Name.StartsWith("A"),
                p => p.IsActive
            };

            // Act
            var count = await repo.GetCountAsync<Product>(conditions);

            // Assert
            Assert.Equal(1, count);
        }

        [Fact]
        public async Task GetCountAsync_WithMultipleConditions_ShouldReturnAll_WhenConditionsIsNull()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetCountAsync_WithMultipleConditions_ShouldReturnAll_WhenConditionsIsNull));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddRangeAsync(
                new Product { Name = "P1" },
                new Product { Name = "P2" });
            await context.SaveChangesAsync();

            // Act
            var count = await repo.GetCountAsync<Product>((IEnumerable<Expression<Func<Product, bool>>>)null);

            // Assert
            Assert.Equal(2, count);
        }

        [Fact]
        public async Task GetCountAsync_WithMultipleConditions_ShouldReturnAll_WhenConditionsIsEmpty()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetCountAsync_WithMultipleConditions_ShouldReturnAll_WhenConditionsIsEmpty));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddRangeAsync(
                new Product { Name = "P1" },
                new Product { Name = "P2" });
            await context.SaveChangesAsync();

            // Act
            var count = await repo.GetCountAsync<Product>(new List<Expression<Func<Product, bool>>>());

            // Assert
            Assert.Equal(2, count);
        }

        [Fact]
        public async Task GetCountAsync_ShouldCancel_WhenCancellationRequested()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetCountAsync_ShouldCancel_WhenCancellationRequested));
            var repo = new Repository<TestDbContext>(context);
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert

            async Task TestMethod() => await repo.GetCountAsync<Product>(cts.Token);
            await Assert.ThrowsAsync<OperationCanceledException>(TestMethod);
        }
        #endregion

        #region GetLongCountAsync
        [Fact]
        public async Task GetLongCountAsync_ShouldReturnCorrectCount()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetLongCountAsync_ShouldReturnCorrectCount));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddRangeAsync(
                new Product { Name = "P1" },
                new Product { Name = "P2" },
                new Product { Name = "P3", IsDeleted = true });
            await context.SaveChangesAsync();

            // Act
            var count = await repo.GetLongCountAsync<Product>();

            // Assert
            Assert.Equal(2L, count);
        }

        [Fact]
        public async Task GetLongCountAsync_ShouldReturnZero_WhenNoEntitiesExist()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetLongCountAsync_ShouldReturnZero_WhenNoEntitiesExist));
            var repo = new Repository<TestDbContext>(context);

            // Act
            var count = await repo.GetLongCountAsync<Product>();

            // Assert
            Assert.Equal(0L, count);
        }

        [Fact]
        public async Task GetLongCountAsync_WithCondition_ShouldReturnFilteredCount()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetLongCountAsync_WithCondition_ShouldReturnFilteredCount));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddRangeAsync(
                new Product { Name = "Apple" },
                new Product { Name = "Banana" },
                new Product { Name = "Cherry" });
            await context.SaveChangesAsync();

            // Act
            var count = await repo.GetLongCountAsync<Product>(p => p.Name.StartsWith("A"));

            // Assert
            Assert.Equal(1L, count);
        }

        [Fact]
        public async Task GetLongCountAsync_WithCondition_ShouldReturnAll_WhenConditionIsNull()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetLongCountAsync_WithCondition_ShouldReturnAll_WhenConditionIsNull));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddRangeAsync(
                new Product { Name = "P1" },
                new Product { Name = "P2" });
            await context.SaveChangesAsync();

            // Act
            var count = await repo.GetLongCountAsync<Product>((Expression<Func<Product, bool>>)null);

            // Assert
            Assert.Equal(2L, count);
        }

        [Fact]
        public async Task GetLongCountAsync_WithMultipleConditions_ShouldReturnFilteredCount()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetLongCountAsync_WithMultipleConditions_ShouldReturnFilteredCount));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddRangeAsync(
                new Product { Name = "Apple", IsActive = true },
                new Product { Name = "Banana", IsActive = true },
                new Product { Name = "Cherry", IsActive = false });
            await context.SaveChangesAsync();
            var conditions = new List<Expression<Func<Product, bool>>>
            {
                p => p.Name.StartsWith("A"),
                p => p.IsActive
            };

            // Act
            var count = await repo.GetLongCountAsync<Product>(conditions);

            // Assert
            Assert.Equal(1L, count);
        }

        [Fact]
        public async Task GetLongCountAsync_WithMultipleConditions_ShouldReturnAll_WhenConditionsIsNull()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetLongCountAsync_WithMultipleConditions_ShouldReturnAll_WhenConditionsIsNull));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddRangeAsync(
                new Product { Name = "P1" },
                new Product { Name = "P2" });
            await context.SaveChangesAsync();

            // Act
            var count = await repo.GetLongCountAsync<Product>((IEnumerable<Expression<Func<Product, bool>>>)null);

            // Assert
            Assert.Equal(2L, count);
        }

        [Fact]
        public async Task GetLongCountAsync_WithMultipleConditions_ShouldReturnAll_WhenConditionsIsEmpty()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetLongCountAsync_WithMultipleConditions_ShouldReturnAll_WhenConditionsIsEmpty));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddRangeAsync(
                new Product { Name = "P1" },
                new Product { Name = "P2" });
            await context.SaveChangesAsync();

            // Act
            var count = await repo.GetLongCountAsync<Product>(new List<Expression<Func<Product, bool>>>());

            // Assert
            Assert.Equal(2L, count);
        }

        [Fact]
        public async Task GetLongCountAsync_ShouldCancel_WhenCancellationRequested()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetLongCountAsync_ShouldCancel_WhenCancellationRequested));
            var repo = new Repository<TestDbContext>(context);
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert

            async Task TestMethod() =>
                await repo.GetLongCountAsync<Product>(cts.Token);
            await Assert.ThrowsAsync<OperationCanceledException>(TestMethod);
        }
        #endregion

        #region HasAnyAsync
        [Fact]
        public async Task HasAnyAsync_ShouldReturnTrue_WhenEntityExists()
        {
            // Arrange
            using var context = GetDbContext(nameof(HasAnyAsync_ShouldReturnTrue_WhenEntityExists));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddAsync(new Product { Name = "Exists" });
            await context.SaveChangesAsync();

            // Act
            var result = await repo.HasAnyAsync<Product>();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task HasAnyAsync_ShouldReturnFalse_WhenNoEntitiesExist()
        {
            // Arrange
            using var context = GetDbContext(nameof(HasAnyAsync_ShouldReturnFalse_WhenNoEntitiesExist));
            var repo = new Repository<TestDbContext>(context);

            // Act
            var result = await repo.HasAnyAsync<Product>();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task HasAnyAsync_WithCondition_ShouldReturnTrue_WhenMatches()
        {
            // Arrange
            using var context = GetDbContext(nameof(HasAnyAsync_WithCondition_ShouldReturnTrue_WhenMatches));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddRangeAsync(
                new Product { Name = "Alpha" },
                new Product { Name = "Beta" });
            await context.SaveChangesAsync();

            // Act
            var result = await repo.HasAnyAsync<Product>(p => p.Name.StartsWith("B"));

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task HasAnyAsync_WithCondition_ShouldReturnFalse_WhenNoMatches()
        {
            // Arrange
            using var context = GetDbContext(nameof(HasAnyAsync_WithCondition_ShouldReturnFalse_WhenNoMatches));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddAsync(new Product { Name = "Alpha" });
            await context.SaveChangesAsync();

            // Act
            var result = await repo.HasAnyAsync<Product>(p => p.Name.StartsWith("B"));

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task HasAnyAsync_WithCondition_ShouldReturnAll_WhenConditionIsNull()
        {
            // Arrange
            using var context = GetDbContext(nameof(HasAnyAsync_WithCondition_ShouldReturnAll_WhenConditionIsNull));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddAsync(new Product { Name = "Exists" });
            await context.SaveChangesAsync();

            // Act
            var result = await repo.HasAnyAsync<Product>((Expression<Func<Product, bool>>)null);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task HasAnyAsync_ShouldReturnFalse_WhenAllEntitiesAreSoftDeleted()
        {
            // Arrange
            using var context = GetDbContext(nameof(HasAnyAsync_ShouldReturnFalse_WhenAllEntitiesAreSoftDeleted));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddAsync(new Product { Name = "Deleted", IsDeleted = true });
            await context.SaveChangesAsync();

            // Act
            var result = await repo.HasAnyAsync<Product>();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task HasAnyAsync_ShouldCancel_WhenCancellationRequested()
        {
            // Arrange
            using var context = GetDbContext(nameof(HasAnyAsync_ShouldCancel_WhenCancellationRequested));
            var repo = new Repository<TestDbContext>(context);
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            async Task TestMethod() =>
                await repo.HasAnyAsync<Product>(cts.Token);
            await Assert.ThrowsAsync<OperationCanceledException>(TestMethod);
        }
        #endregion

        #region CountDeletedAsync
        [Fact]
        public async Task CountDeletedAsync_ShouldReturnOnlyDeletedEntities()
        {
            // Arrange
            using var context = GetDbContext(nameof(CountDeletedAsync_ShouldReturnOnlyDeletedEntities));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddRangeAsync(
                new Product { Name = "Active" },
                new Product { Name = "Deleted", IsDeleted = true });
            await context.SaveChangesAsync();

            // Act
            var count = await repo.CountDeletedAsync<Product>();

            // Assert
            Assert.Equal(1, count);
        }

        [Fact]
        public async Task CountDeletedAsync_ShouldReturnZero_WhenNoDeletedEntities()
        {
            // Arrange
            using var context = GetDbContext(nameof(CountDeletedAsync_ShouldReturnZero_WhenNoDeletedEntities));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddAsync(new Product { Name = "Active" });
            await context.SaveChangesAsync();

            // Act
            var count = await repo.CountDeletedAsync<Product>();

            // Assert
            Assert.Equal(0, count);
        }

        [Fact]
        public async Task CountDeletedAsync_ShouldCancel_WhenCancellationRequested()
        {
            // Arrange
            using var context = GetDbContext(nameof(CountDeletedAsync_ShouldCancel_WhenCancellationRequested));
            var repo = new Repository<TestDbContext>(context);
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            async Task TestMethod() =>
                await repo.CountDeletedAsync<Product>(cts.Token);
            await Assert.ThrowsAsync<OperationCanceledException>(TestMethod);
        }
        #endregion

        #region CountSoftDeletedAsync
        [Fact]
        public async Task CountSoftDeletedAsync_ShouldReturnCorrectCount()
        {
            // Arrange
            using var context = GetDbContext(nameof(CountSoftDeletedAsync_ShouldReturnCorrectCount));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddRangeAsync(
                new Product { Name = "Active" },
                new Product { Name = "Deleted", IsDeleted = true });
            await context.SaveChangesAsync();

            // Act
            var count = await repo.CountSoftDeletedAsync<Product>();

            // Assert
            Assert.Equal(1, count);
        }

        [Fact]
        public async Task CountSoftDeletedAsync_ShouldReturnZero_WhenNoDeletedEntities()
        {
            // Arrange
            using var context = GetDbContext(nameof(CountSoftDeletedAsync_ShouldReturnZero_WhenNoDeletedEntities));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddAsync(new Product { Name = "Active" });
            await context.SaveChangesAsync();

            // Act
            var count = await repo.CountSoftDeletedAsync<Product>();

            // Assert
            Assert.Equal(0, count);
        }
        #endregion

        #region CountByStatusAsync
        [Fact]
        public async Task CountByStatusAsync_ShouldReturnGroupedCounts()
        {
            // Arrange
            using var context = GetDbContext(nameof(CountByStatusAsync_ShouldReturnGroupedCounts));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddRangeAsync(
                new Product { Name = "Active", IsActive = true },
                new Product { Name = "Inactive", IsActive = false },
                new Product { Name = "AlsoActive", IsActive = true });
            await context.SaveChangesAsync();

            // Act
            var result = await repo.CountByStatusAsync<Product>(p => p.IsActive);

            // Assert
            Assert.Equal(2, result[true]);
            Assert.Equal(1, result[false]);
        }

        [Fact]
        public async Task CountByStatusAsync_ShouldThrow_WhenStatusSelectorIsNull()
        {
            // Arrange
            using var context = GetDbContext(nameof(CountByStatusAsync_ShouldThrow_WhenStatusSelectorIsNull));
            var repo = new Repository<TestDbContext>(context);

            // Act & Assert
            async Task TestMethod() =>
                await repo.CountByStatusAsync<Product>((Expression<Func<Product, bool>>)null);
            await Assert.ThrowsAsync<ArgumentNullException>(TestMethod);
        }

        [Fact]
        public async Task CountByStatusAsync_ShouldReturnEmptyDictionary_WhenNoEntitiesExist()
        {
            // Arrange
            using var context = GetDbContext(nameof(CountByStatusAsync_ShouldReturnEmptyDictionary_WhenNoEntitiesExist));
            var repo = new Repository<TestDbContext>(context);

            // Act
            var result = await repo.CountByStatusAsync<Product>(p => p.IsActive);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task CountByStatusAsync_ShouldExcludeSoftDeletedEntities()
        {
            // Arrange
            using var context = GetDbContext(nameof(CountByStatusAsync_ShouldExcludeSoftDeletedEntities));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddRangeAsync(
                new Product { Name = "Active", IsActive = true },
                new Product { Name = "Deleted", IsActive = true, IsDeleted = true });
            await context.SaveChangesAsync();

            // Act
            var result = await repo.CountByStatusAsync<Product>(p => p.IsActive);

            // Assert
            Assert.Equal(1, result[true]);
            Assert.False(result.ContainsKey(false));
        }

        [Fact]
        public async Task CountByStatusAsync_ShouldCancel_WhenCancellationRequested()
        {
            // Arrange
            using var context = GetDbContext(nameof(CountByStatusAsync_ShouldCancel_WhenCancellationRequested));
            var repo = new Repository<TestDbContext>(context);
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            async Task TestMethod() =>
                await repo.CountByStatusAsync<Product>(p => p.IsActive, cts.Token);
            await Assert.ThrowsAsync<OperationCanceledException>(TestMethod);
        }
        #endregion

        #region CountByDateRangeAsync
        [Fact]
        public async Task CountByDateRangeAsync_ShouldReturnCorrectCount()
        {
            // Arrange
            using var context = GetDbContext(nameof(CountByDateRangeAsync_ShouldReturnCorrectCount));
            var repo = new Repository<TestDbContext>(context);
            var now = DateTime.UtcNow;
            await context.Products.AddRangeAsync(
                new Product { Name = "Old", CreatedOnUtc = now.AddDays(-10) },
                new Product { Name = "Recent", CreatedOnUtc = now.AddDays(-1) });
            await context.SaveChangesAsync();

            // Act
            var count = await repo.CountByDateRangeAsync<Product>(now.AddDays(-5), now);

            // Assert
            Assert.Equal(1, count);
        }

        [Fact]
        public async Task CountByDateRangeAsync_ShouldReturnZero_WhenNoEntitiesInRange()
        {
            // Arrange
            using var context = GetDbContext(nameof(CountByDateRangeAsync_ShouldReturnZero_WhenNoEntitiesInRange));
            var repo = new Repository<TestDbContext>(context);
            var now = DateTime.UtcNow;
            await context.Products.AddAsync(new Product { Name = "Old", CreatedOnUtc = now.AddDays(-10) });
            await context.SaveChangesAsync();

            // Act
            var count = await repo.CountByDateRangeAsync<Product>(now.AddDays(-5), now.AddDays(-2));

            // Assert
            Assert.Equal(0, count);
        }

        [Fact]
        public async Task CountByDateRangeAsync_ShouldReturnZero_WhenStartDateAfterEndDate()
        {
            // Arrange
            using var context = GetDbContext(nameof(CountByDateRangeAsync_ShouldReturnZero_WhenStartDateAfterEndDate));
            var repo = new Repository<TestDbContext>(context);
            var now = DateTime.UtcNow;
            await context.Products.AddAsync(new Product { Name = "Recent", CreatedOnUtc = now });
            await context.SaveChangesAsync();

            // Act
            var count = await repo.CountByDateRangeAsync<Product>(now.AddDays(1), now);

            // Assert
            Assert.Equal(0, count);
        }

        [Fact]
        public async Task CountByDateRangeAsync_ShouldExcludeSoftDeletedEntities()
        {
            // Arrange
            using var context = GetDbContext(nameof(CountByDateRangeAsync_ShouldExcludeSoftDeletedEntities));
            var repo = new Repository<TestDbContext>(context);
            var now = DateTime.UtcNow;
            await context.Products.AddAsync(new Product { Name = "Deleted", CreatedOnUtc = now, IsDeleted = true });
            await context.SaveChangesAsync();

            // Act
            var count = await repo.CountByDateRangeAsync<Product>(now.AddDays(-1), now.AddDays(1));

            // Assert
            Assert.Equal(0, count);
        }

        [Fact]
        public async Task CountByDateRangeAsync_ShouldCancel_WhenCancellationRequested()
        {
            // Arrange
            using var context = GetDbContext(nameof(CountByDateRangeAsync_ShouldCancel_WhenCancellationRequested));
            var repo = new Repository<TestDbContext>(context);
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            async Task TestMethod() =>
                await repo.CountByDateRangeAsync<Product>(DateTime.UtcNow, DateTime.UtcNow, cts.Token);
            await Assert.ThrowsAsync<OperationCanceledException>(TestMethod);
        }
        #endregion 
    }
}