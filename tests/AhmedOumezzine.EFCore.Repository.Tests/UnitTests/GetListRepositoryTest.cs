using AhmedOumezzine.EFCore.Repository.Entities;
using AhmedOumezzine.EFCore.Repository.Repository;
using AhmedOumezzine.EFCore.Repository.Specification;
using AhmedOumezzine.EFCore.Repository.Tests.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AhmedOumezzine.EFCore.Repository.Tests.UnitTests
{
    public class GetListRepositoryTests
    {
        private TestDbContext GetDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            return new TestDbContext(options);
        }

        #region GetListAsync
        [Fact]
        public async Task GetListAsync_ShouldReturnAllNonDeletedEntities()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetListAsync_ShouldReturnAllNonDeletedEntities));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddRangeAsync(
                new Product { Name = "P1" },
                new Product { Name = "P2", IsDeleted = true },
                new Product { Name = "P3" });
            await context.SaveChangesAsync();

            // Act
            var list = await repo.GetListAsync<Product>();

            // Assert
            Assert.Equal(2, list.Count);
            Assert.All(list, p => Assert.False(p.IsDeleted));
        }

        [Fact]
        public async Task GetListAsync_ShouldReturnEmptyList_WhenNoEntitiesExist()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetListAsync_ShouldReturnEmptyList_WhenNoEntitiesExist));
            var repo = new Repository<TestDbContext>(context);

            // Act
            var list = await repo.GetListAsync<Product>();

            // Assert
            Assert.Empty(list);
        }

        [Fact]
        public async Task GetListAsync_AsNoTracking_ShouldReturnDetachedEntities()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetListAsync_AsNoTracking_ShouldReturnDetachedEntities));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddAsync(new Product { Name = "P1" });
            await context.SaveChangesAsync();

            // Act
            var list = await repo.GetListAsync<Product>(asNoTracking: true);

            // Assert
            Assert.Single(list);
            Assert.Equal(EntityState.Detached, context.Entry(list.First()).State);
        }

        [Fact]
        public async Task GetListAsync_AsNoTracking_ShouldCancel_WhenCancellationRequested()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetListAsync_AsNoTracking_ShouldCancel_WhenCancellationRequested));
            var repo = new Repository<TestDbContext>(context);
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            async Task TestMethod() =>
                await repo.GetListAsync<Product>(asNoTracking: true, cancellationToken: cts.Token);
            await Assert.ThrowsAsync<OperationCanceledException>(TestMethod);
        }

        [Fact]
        public async Task GetListAsync_WithIncludes_ShouldReturnEntitiesWithRelatedData()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetListAsync_WithIncludes_ShouldReturnEntitiesWithRelatedData));
            var repo = new Repository<TestDbContext>(context);
            var product = new Product { Name = "P1", Category = new Category { Name = "Electronics" } };
            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();

            // Act
            var list = await repo.GetListAsync<Product>(q => q.Include(p => p.Category));

            // Assert
            Assert.Single(list);
            Assert.Equal("Electronics", list.First().Category.Name);
        }

        [Fact]
        public async Task GetListAsync_WithCondition_ShouldReturnFilteredList()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetListAsync_WithCondition_ShouldReturnFilteredList));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddRangeAsync(
                new Product { Name = "Alpha" },
                new Product { Name = "Beta" },
                new Product { Name = "AlphaX" });
            await context.SaveChangesAsync();
            Expression<Func<Product, bool>> condition = p => p.Name.StartsWith("Alpha");

            // Act
            var list = await repo.GetListAsync(condition);

            // Assert
            Assert.Equal(2, list.Count);
            Assert.All(list, p => Assert.StartsWith("Alpha", p.Name));
        }

        [Fact]
        public async Task GetListAsync_WithCondition_ShouldReturnAll_WhenConditionIsNull()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetListAsync_WithCondition_ShouldReturnAll_WhenConditionIsNull));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddRangeAsync(
                new Product { Name = "P1" },
                new Product { Name = "P2" });
            await context.SaveChangesAsync();

            // Act
            var list = await repo.GetListAsync<Product>((Expression<Func<Product, bool>>)null);

            // Assert
            Assert.Equal(2, list.Count);
        }

        [Fact]
        public async Task GetListAsync_WithCondition_ShouldCancel_WhenCancellationRequested()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetListAsync_WithCondition_ShouldCancel_WhenCancellationRequested));
            var repo = new Repository<TestDbContext>(context);
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            async Task TestMethod() =>
                await repo.GetListAsync<Product>(p => p.Name == "Test", cts.Token);
            await Assert.ThrowsAsync<OperationCanceledException>(TestMethod);
        }

        [Fact]
        public async Task GetListAsync_WithSpecification_ShouldReturnFilteredEntities()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetListAsync_WithSpecification_ShouldReturnFilteredEntities));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddRangeAsync(
                new Product { Name = "P1", IsActive = true },
                new Product { Name = "P2", IsActive = false });
            await context.SaveChangesAsync();
            var spec = new Specification<Product>();
            spec.Conditions.Add( p => p.IsActive );
            // Act
            var list = await repo.GetListAsync(spec);

            // Assert
            Assert.Single(list);
            Assert.True(list.First().IsActive);
        }

        [Fact]
        public async Task GetListAsync_WithSpecification_ShouldThrow_WhenSpecificationIsNull()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetListAsync_WithSpecification_ShouldThrow_WhenSpecificationIsNull));
            var repo = new Repository<TestDbContext>(context);

            // Act & Assert
            async Task TestMethod() =>
                await repo.GetListAsync<Product>((Specification<Product>)null);
            await Assert.ThrowsAsync<ArgumentNullException>(TestMethod);
        }

        [Fact]
        public async Task GetListAsync_WithProjection_ShouldReturnDtoList()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetListAsync_WithProjection_ShouldReturnDtoList));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddRangeAsync(
                new Product { Name = "P1", Price = 100 },
                new Product { Name = "P2", Price = 200 });
            await context.SaveChangesAsync();

            // Act
            var projected = await repo.GetListAsync<Product, ProductDto>(p => new ProductDto { Name = p.Name, Price = p.Price });

            // Assert
            Assert.Equal(2, projected.Count);
            Assert.Contains(projected, p => p.Name == "P1" && p.Price == 100);
            Assert.Contains(projected, p => p.Name == "P2" && p.Price == 200);
        }

        [Fact]
        public async Task GetListAsync_WithProjection_ShouldThrow_WhenSelectExpressionIsNull()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetListAsync_WithProjection_ShouldThrow_WhenSelectExpressionIsNull));
            var repo = new Repository<TestDbContext>(context);

            // Act & Assert
            async Task TestMethod() =>
                await repo.GetListAsync<Product, ProductDto>((Expression<Func<Product, ProductDto>>)null);
            await Assert.ThrowsAsync<ArgumentNullException>(TestMethod);
        }

        [Fact]
        public async Task GetListAsync_WithConditionAndProjection_ShouldReturnFilteredDtoList()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetListAsync_WithConditionAndProjection_ShouldReturnFilteredDtoList));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddRangeAsync(
                new Product { Name = "P1", Price = 150 },
                new Product { Name = "P2", Price = 50 });
            await context.SaveChangesAsync();

            // Act
            var projected = await repo.GetListAsync<Product, ProductDto>(
                p => p.Price > 100,
                p => new ProductDto { Name = p.Name, Price = p.Price });

            // Assert
            Assert.Single(projected);
            Assert.Equal("P1", projected.First().Name);
        }

        [Fact]
        public async Task GetListAsync_WithConditionAndProjection_ShouldThrow_WhenSelectExpressionIsNull()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetListAsync_WithConditionAndProjection_ShouldThrow_WhenSelectExpressionIsNull));
            var repo = new Repository<TestDbContext>(context);

            // Act & Assert
            async Task TestMethod() =>
                await repo.GetListAsync<Product, ProductDto>(p => p.Price > 100, null);
            await Assert.ThrowsAsync<ArgumentNullException>(TestMethod);
        }

        [Fact]
        public async Task GetListAsync_WithPagination_ShouldReturnPaginatedList()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetListAsync_WithPagination_ShouldReturnPaginatedList));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddRangeAsync(
                new Product { Name = "P1" },
                new Product { Name = "P2" },
                new Product { Name = "P3" });
            await context.SaveChangesAsync();
            var spec = new PaginationSpecification<Product>( );
            spec.PageIndex = 1;
            spec.PageSize = 2;
            // Act
            var result = await repo.GetListAsync(spec);

            // Assert
            Assert.Equal(2, result.Items.Count);
            Assert.Equal(3, result.TotalPages); 
        }

        [Fact]
        public async Task GetListAsync_WithPagination_ShouldThrow_WhenSpecificationIsNull()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetListAsync_WithPagination_ShouldThrow_WhenSpecificationIsNull));
            var repo = new Repository<TestDbContext>(context);

            // Act & Assert
            async Task TestMethod() =>
                await repo.GetListAsync<Product>((PaginationSpecification<Product>)null);
            await Assert.ThrowsAsync<ArgumentNullException>(TestMethod);
        }

        [Fact]
        public async Task GetListAsync_WithPagination_ShouldThrow_WhenPageIndexIsInvalid()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetListAsync_WithPagination_ShouldThrow_WhenPageIndexIsInvalid));
            var repo = new Repository<TestDbContext>(context);
            var spec = new PaginationSpecification<Product>( );
            spec.PageIndex = 0;
            spec.PageSize = 10;

            // Act & Assert
            async Task TestMethod() =>
                await repo.GetListAsync(spec);
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(TestMethod);
        }

        [Fact]
        public async Task GetListAsync_WithPagination_ShouldThrow_WhenPageSizeIsInvalid()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetListAsync_WithPagination_ShouldThrow_WhenPageSizeIsInvalid));
            var repo = new Repository<TestDbContext>(context);
            var spec = new PaginationSpecification<Product>( );
            spec.PageIndex = 1;
            spec.PageSize = 0;

            // Act & Assert
            async Task TestMethod() =>
                await repo.GetListAsync(spec);
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(TestMethod);
        }

        [Fact]
        public async Task GetListAsync_WithPagination_ShouldCancel_WhenCancellationRequested()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetListAsync_WithPagination_ShouldCancel_WhenCancellationRequested));
            var repo = new Repository<TestDbContext>(context);
            var spec = new PaginationSpecification<Product>( );
            spec.PageIndex = 1;
            spec.PageSize = 10;
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            async Task TestMethod() =>
                await repo.GetListAsync(spec, cts.Token);
            await Assert.ThrowsAsync<OperationCanceledException>(TestMethod);
        }

        [Fact]
        public async Task GetListAsync_WithPaginationAndProjection_ShouldReturnPaginatedDtoList()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetListAsync_WithPaginationAndProjection_ShouldReturnPaginatedDtoList));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddRangeAsync(
                new Product { Name = "P1", Price = 100 },
                new Product { Name = "P2", Price = 200 },
                new Product { Name = "P3", Price = 300 });
            await context.SaveChangesAsync();
            var spec = new PaginationSpecification<Product>( );

            spec.PageIndex = 1;
            spec.PageSize = 2;
            // Act
            var result = await repo.GetListAsync<Product, ProductDto>(
                spec,
                p => new ProductDto { Name = p.Name, Price = p.Price });

            // Assert
            Assert.Equal(2, result.Items.Count);
            Assert.Equal(3, result.TotalPages);
            Assert.Contains(result.Items, p => p.Name == "P1" && p.Price == 100);
        }

        [Fact]
        public async Task GetListAsync_WithPaginationAndProjection_ShouldThrow_WhenSelectExpressionIsNull()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetListAsync_WithPaginationAndProjection_ShouldThrow_WhenSelectExpressionIsNull));
            var repo = new Repository<TestDbContext>(context);
            var spec = new PaginationSpecification<Product>( );


            spec.PageIndex = 1;
            spec.PageSize = 10;
            // Act & Assert
            async Task TestMethod() =>
                await repo.GetListAsync<Product, ProductDto>(spec, null);
            await Assert.ThrowsAsync<ArgumentNullException>(TestMethod );
        }

        [Fact]
        public async Task GetListAsync_WithPaginationAndProjection_ShouldCancel_WhenCancellationRequested()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetListAsync_WithPaginationAndProjection_ShouldCancel_WhenCancellationRequested));
            var repo = new Repository<TestDbContext>(context);
            var spec = new PaginationSpecification<Product>( );
            spec.PageIndex = 1;
            spec.PageSize = 10;
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            async Task TestMethod() =>
                await repo.GetListAsync<Product, ProductDto>(
                    spec,
                    p => new ProductDto { Name = p.Name, Price = p.Price },
                    cts.Token);
            await Assert.ThrowsAsync<OperationCanceledException>(TestMethod);
        }
        #endregion

        #region GetDeletedListAsync
        [Fact]
        public async Task GetDeletedListAsync_ShouldReturnOnlyDeletedEntities()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetDeletedListAsync_ShouldReturnOnlyDeletedEntities));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddRangeAsync(
                new Product { Name = "Active" },
                new Product { Name = "Deleted1", IsDeleted = true },
                new Product { Name = "Deleted2", IsDeleted = true });
            await context.SaveChangesAsync();

            // Act
            var deleted = await repo.GetDeletedListAsync<Product>();

            // Assert
            Assert.Equal(2, deleted.Count);
            Assert.All(deleted, p => Assert.True(p.IsDeleted));
        }

        [Fact]
        public async Task GetDeletedListAsync_ShouldReturnEmptyList_WhenNoDeletedEntities()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetDeletedListAsync_ShouldReturnEmptyList_WhenNoDeletedEntities));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddAsync(new Product { Name = "Active" });
            await context.SaveChangesAsync();

            // Act
            var deleted = await repo.GetDeletedListAsync<Product>();

            // Assert
            Assert.Empty(deleted);
        }

        [Fact]
        public async Task GetDeletedListAsync_ShouldCancel_WhenCancellationRequested()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetDeletedListAsync_ShouldCancel_WhenCancellationRequested));
            var repo = new Repository<TestDbContext>(context);
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            async Task TestMethod() => 
                await repo.GetDeletedListAsync<Product>(null,false,cts.Token);
            await Assert.ThrowsAsync<OperationCanceledException>(TestMethod);
        }
        #endregion

        #region GetDistinctByAsync
        [Fact]
        public async Task GetDistinctByAsync_ShouldReturnDistinctValues()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetDistinctByAsync_ShouldReturnDistinctValues));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddRangeAsync(
                new Product { Name = "A", Category  = new Category { Name = "Electronics" }  },
                new Product { Name = "B", Category = new Category { Name = "Electronics" }  },
                new Product { Name = "C", Category = new Category { Name = "Books" }   });
            await context.SaveChangesAsync();

            // Act
            var distinct = await repo.GetDistinctByAsync<Product, Category>(p => p.Category);

            // Assert
            Assert.Equal(2, distinct.Count);
            Assert.Contains("Electronics", distinct.FirstOrDefault().Name);
            Assert.Contains("Books", distinct.LastOrDefault().Name);
        }

        [Fact]
        public async Task GetDistinctByAsync_ShouldExcludeSoftDeletedEntities()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetDistinctByAsync_ShouldExcludeSoftDeletedEntities));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddRangeAsync(
                new Product { Name = "A", Category = new Category { Name = "Electronics" } },
                new Product { Name = "B", Category = new Category { Name = "Books" }  , IsDeleted = true });
            await context.SaveChangesAsync();

            // Act
            var distinct = await repo.GetDistinctByAsync<Product, Category>(p => p.Category);

            // Assert
            Assert.Single(distinct);
            Assert.Contains("Electronics", distinct.FirstOrDefault().Name);
        }

        [Fact]
        public async Task GetDistinctByAsync_ShouldThrow_WhenKeySelectorIsNull()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetDistinctByAsync_ShouldThrow_WhenKeySelectorIsNull));
            var repo = new Repository<TestDbContext>(context);

            // Act & Assert
            async Task TestMethod() =>
                await repo.GetDistinctByAsync<Product, string>(null);
            await Assert.ThrowsAsync<ArgumentNullException>(TestMethod);
        }

        [Fact]
        public async Task GetDistinctByAsync_ShouldCancel_WhenCancellationRequested()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetDistinctByAsync_ShouldCancel_WhenCancellationRequested));
            var repo = new Repository<TestDbContext>(context);
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            async Task TestMethod() =>
                await repo.GetDistinctByAsync<Product, Category>(p => p.Category, cts.Token);
            await Assert.ThrowsAsync<OperationCanceledException>(TestMethod);
        }
        #endregion

        #region TryGetListAsync
        [Fact]
        public async Task TryGetListAsync_ShouldReturnSuccessAndItems_WhenEntitiesExist()
        {
            // Arrange
            using var context = GetDbContext(nameof(TryGetListAsync_ShouldReturnSuccessAndItems_WhenEntitiesExist));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddAsync(new Product { Name = "Test" });
            await context.SaveChangesAsync();

            // Act
            var (success, items) = await repo.TryGetListAsync<Product>();

            // Assert
            Assert.True(success);
            Assert.Single(items);
        }

        [Fact]
        public async Task TryGetListAsync_ShouldReturnSuccessAndEmptyList_WhenNoEntitiesExist()
        {
            // Arrange
            using var context = GetDbContext(nameof(TryGetListAsync_ShouldReturnSuccessAndEmptyList_WhenNoEntitiesExist));
            var repo = new Repository<TestDbContext>(context);

            // Act
            var (success, items) = await repo.TryGetListAsync<Product>();

            // Assert
            Assert.True(success);
            Assert.Empty(items);
        }

        [Fact]
        public async Task TryGetListAsync_WithCondition_ShouldReturnFilteredItems()
        {
            // Arrange
            using var context = GetDbContext(nameof(TryGetListAsync_WithCondition_ShouldReturnFilteredItems));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddRangeAsync(
                new Product { Name = "Alpha" },
                new Product { Name = "Beta" });
            await context.SaveChangesAsync();

            // Act
            var (success, items) = await repo.TryGetListAsync<Product>(p => p.Name == "Alpha");

            // Assert
            Assert.True(success);
            Assert.Single(items);
            Assert.Equal("Alpha", items.First().Name);
        }

        [Fact]
        public async Task TryGetListAsync_WithCondition_ShouldReturnAll_WhenConditionIsNull()
        {
            // Arrange
            using var context = GetDbContext(nameof(TryGetListAsync_WithCondition_ShouldReturnAll_WhenConditionIsNull));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddRangeAsync(
                new Product { Name = "P1" },
                new Product { Name = "P2" });
            await context.SaveChangesAsync();

            // Act
            var (success, items) = await repo.TryGetListAsync<Product>((Expression<Func<Product, bool>>)null);

            // Assert
            Assert.True(success);
            Assert.Equal(2, items.Count);
        }

        [Fact]
        public async Task TryGetListAsync_ShouldCancel_WhenCancellationRequested()
        {
            // Arrange
            using var context = GetDbContext(nameof(TryGetListAsync_ShouldCancel_WhenCancellationRequested));
            var repo = new Repository<TestDbContext>(context);
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            async Task TestMethod() =>
                await repo.TryGetListAsync<Product>(null,cts.Token);
            await Assert.ThrowsAsync<OperationCanceledException>(TestMethod);
        }
        #endregion

        #region ExistsAnyAndListAsync
        [Fact]
        public async Task ExistsAnyAndListAsync_ShouldReturnCorrectTuple_WhenEntitiesExist()
        {
            // Arrange
            using var context = GetDbContext(nameof(ExistsAnyAndListAsync_ShouldReturnCorrectTuple_WhenEntitiesExist));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddRangeAsync(
                new Product { Name = "X" },
                new Product { Name = "Y" });
            await context.SaveChangesAsync();

            // Act
            var (hasAny, items) = await repo.ExistsAnyAndListAsync<Product>(p => p.Name.StartsWith("X"));

            // Assert
            Assert.True(hasAny);
            Assert.Single(items);
            Assert.Equal("X", items.First().Name);
        }

        [Fact]
        public async Task ExistsAnyAndListAsync_ShouldReturnFalseAndEmptyList_WhenNoEntitiesMatch()
        {
            // Arrange
            using var context = GetDbContext(nameof(ExistsAnyAndListAsync_ShouldReturnFalseAndEmptyList_WhenNoEntitiesMatch));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddAsync(new Product { Name = "Y" });
            await context.SaveChangesAsync();

            // Act
            var (hasAny, items) = await repo.ExistsAnyAndListAsync<Product>(p => p.Name.StartsWith("X"));

            // Assert
            Assert.False(hasAny);
            Assert.Empty(items);
        }

        [Fact]
        public async Task ExistsAnyAndListAsync_ShouldReturnAll_WhenConditionIsNull()
        {
            // Arrange
            using var context = GetDbContext(nameof(ExistsAnyAndListAsync_ShouldReturnAll_WhenConditionIsNull));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddRangeAsync(
                new Product { Name = "P1" },
                new Product { Name = "P2" });
            await context.SaveChangesAsync();

            // Act
            var (hasAny, items) = await repo.ExistsAnyAndListAsync<Product>((Expression<Func<Product, bool>>)null);

            // Assert
            Assert.True(hasAny);
            Assert.Equal(2, items.Count);
        }

        [Fact]
        public async Task ExistsAnyAndListAsync_ShouldCancel_WhenCancellationRequested()
        {
            // Arrange
            using var context = GetDbContext(nameof(ExistsAnyAndListAsync_ShouldCancel_WhenCancellationRequested));
            var repo = new Repository<TestDbContext>(context);
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            async Task TestMethod() =>
                await repo.ExistsAnyAndListAsync<Product>(p => p.Name == "Test", cts.Token);
            await Assert.ThrowsAsync<OperationCanceledException>(TestMethod);
        }
        #endregion

     
    }
}