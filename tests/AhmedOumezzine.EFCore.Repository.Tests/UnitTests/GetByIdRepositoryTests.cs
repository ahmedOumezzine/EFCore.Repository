using AhmedOumezzine.EFCore.Repository.Entities;
using AhmedOumezzine.EFCore.Repository.Repository;
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
    public class GetByIdRepositoryTests
    {
        private TestDbContext GetDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            return new TestDbContext(options);
        }

        #region GetByIdAsync (Guid)
        [Fact]
        public async Task GetByIdAsync_Guid_ShouldReturnEntity_WhenExists()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetByIdAsync_Guid_ShouldReturnEntity_WhenExists));
            var repo = new Repository<TestDbContext>(context);
            var product = new Product { Name = "TestProduct" };
            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();

            // Act
            var result = await repo.GetByIdAsync<Product>(product.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(product.Name, result.Name);
        }

        [Fact]
        public async Task GetByIdAsync_Guid_ShouldReturnNull_WhenNotExists()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetByIdAsync_Guid_ShouldReturnNull_WhenNotExists));
            var repo = new Repository<TestDbContext>(context);

            // Act
            var result = await repo.GetByIdAsync<Product>(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_Guid_ShouldThrow_WhenIdIsNull()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetByIdAsync_Guid_ShouldThrow_WhenIdIsNull));
            var repo = new Repository<TestDbContext>(context);

            // Act & Assert
            async Task TestMethod() =>
                await repo.GetByIdAsync<Product>((Guid?)null);
            await Assert.ThrowsAsync<ArgumentNullException>(TestMethod);
        }

        [Fact]
        public async Task GetByIdAsync_Guid_ShouldReturnNull_WhenEntityIsSoftDeleted()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetByIdAsync_Guid_ShouldReturnNull_WhenEntityIsSoftDeleted));
            var repo = new Repository<TestDbContext>(context);
            var product = new Product { Name = "TestProduct", IsDeleted = true };
            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();

            // Act
            var result = await repo.GetByIdAsync<Product>(product.Id);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_Guid_ShouldCancel_WhenCancellationRequested()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetByIdAsync_Guid_ShouldCancel_WhenCancellationRequested));
            var repo = new Repository<TestDbContext>(context);
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            async Task TestMethod() =>
                await repo.GetByIdAsync<Product>(Guid.NewGuid(), cancellationToken: cts.Token);
            await Assert.ThrowsAsync<OperationCanceledException>(TestMethod);
        }
        #endregion

        #region GetByIdAsync (Guid, Includes)
        [Fact]
        public async Task GetByIdAsync_Guid_WithIncludes_ShouldReturnEntityWithRelatedData()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetByIdAsync_Guid_WithIncludes_ShouldReturnEntityWithRelatedData));
            var repo = new Repository<TestDbContext>(context);
            var customer = new Customer { Name = "TestCustomer" };
            var order = new Order { Description = "TestOrder", CustomerId = customer.Id };
            customer.Orders.Add(order);
            await context.Customers.AddAsync(customer);
            await context.Orders.AddAsync(order);
            await context.SaveChangesAsync();

            // Act
            var result = await repo.GetByIdAsync<Customer>(
                customer.Id,
                q => q.Include(c => c.Orders));

            // Assert
            Assert.NotNull(result);
            Assert.Equal(customer.Name, result.Name);
            Assert.Single(result.Orders);
            Assert.Equal(order.Description, result.Orders.First().Description);
        }

        [Fact]
        public async Task GetByIdAsync_Guid_WithIncludes_ShouldReturnNull_WhenNotExists()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetByIdAsync_Guid_WithIncludes_ShouldReturnNull_WhenNotExists));
            var repo = new Repository<TestDbContext>(context);

            // Act
            var result = await repo.GetByIdAsync<Product>(
                Guid.NewGuid(),
                q => q.Include(p => p.Name));

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_Guid_WithIncludes_ShouldThrow_WhenIdIsNull()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetByIdAsync_Guid_WithIncludes_ShouldThrow_WhenIdIsNull));
            var repo = new Repository<TestDbContext>(context);

            // Act & Assert
            async Task TestMethod() =>
                await repo.GetByIdAsync<Product>((Guid?)null, q => q.Include(p => p.Name));
            await Assert.ThrowsAsync<ArgumentNullException>(TestMethod);
        }
        #endregion

        #region GetByIdAsync (Guid, AsNoTracking)
        [Fact]
        public async Task GetByIdAsync_Guid_AsNoTracking_ShouldReturnEntity()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetByIdAsync_Guid_AsNoTracking_ShouldReturnEntity));
            var repo = new Repository<TestDbContext>(context);
            var product = new Product { Name = "NoTrackingProduct" };
            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();

            // Act
            var result = await repo.GetByIdAsync<Product>(product.Id, asNoTracking: true);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(product.Name, result.Name);
            Assert.Equal(EntityState.Detached, context.Entry(result).State);
        }

        [Fact]
        public async Task GetByIdAsync_Guid_AsNoTracking_ShouldReturnNull_WhenNotExists()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetByIdAsync_Guid_AsNoTracking_ShouldReturnNull_WhenNotExists));
            var repo = new Repository<TestDbContext>(context);

            // Act
            var result = await repo.GetByIdAsync<Product>(Guid.NewGuid(), asNoTracking: true);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_Guid_AsNoTracking_ShouldThrow_WhenIdIsNull()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetByIdAsync_Guid_AsNoTracking_ShouldThrow_WhenIdIsNull));
            var repo = new Repository<TestDbContext>(context);

            // Act & Assert
            async Task TestMethod() =>
                await repo.GetByIdAsync<Product>((Guid?)null, asNoTracking: true);
            await Assert.ThrowsAsync<ArgumentNullException>(TestMethod);
        }
        #endregion

        #region GetByIdAsync (Guid, Includes, AsNoTracking)
        [Fact]
        public async Task GetByIdAsync_Guid_WithIncludesAndNoTracking_ShouldReturnEntity()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetByIdAsync_Guid_WithIncludesAndNoTracking_ShouldReturnEntity));
            var repo = new Repository<TestDbContext>(context);
            var customer = new Customer { Name = "TestCustomer" };
            var order = new Order { Description = "TestOrder", CustomerId = customer.Id };
            customer.Orders.Add(order);
            await context.Customers.AddAsync(customer);
            await context.Orders.AddAsync(order);
            await context.SaveChangesAsync();

            // Act
            var result = await repo.GetByIdAsync<Customer>(
                customer.Id,
                q => q.Include(c => c.Orders),
                asNoTracking: true);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(customer.Name, result.Name);
            Assert.Single(result.Orders);
            Assert.Equal(EntityState.Detached, context.Entry(result).State);
        }

        [Fact]
        public async Task GetByIdAsync_Guid_WithIncludesAndNoTracking_ShouldReturnNull_WhenNotExists()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetByIdAsync_Guid_WithIncludesAndNoTracking_ShouldReturnNull_WhenNotExists));
            var repo = new Repository<TestDbContext>(context);

            // Act
            var result = await repo.GetByIdAsync<Product>(
                Guid.NewGuid(),
                q => q.Include(p => p.Name),
                asNoTracking: true);

            // Assert
            Assert.Null(result);
        }
        #endregion

        #region GetByIdAsync (Projected)
        [Fact]
        public async Task GetByIdAsync_Projected_ShouldReturnProjectedEntity()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetByIdAsync_Projected_ShouldReturnProjectedEntity));
            var repo = new Repository<TestDbContext>(context);
            var product = new Product { Name = "TestProduct" };
            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();

            // Act
            var result = await repo.GetByIdAsync<Product, string>(
                product.Id,
                p => p.Name);

            // Assert
            Assert.Equal(product.Name, result);
        }

        [Fact]
        public async Task GetByIdAsync_Projected_ShouldReturnNull_WhenNotExists()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetByIdAsync_Projected_ShouldReturnNull_WhenNotExists));
            var repo = new Repository<TestDbContext>(context);

            // Act
            var result = await repo.GetByIdAsync<Product, string>(
                Guid.NewGuid(),
                p => p.Name);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_Projected_ShouldThrow_WhenIdIsNull()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetByIdAsync_Projected_ShouldThrow_WhenIdIsNull));
            var repo = new Repository<TestDbContext>(context);

            // Act & Assert
            async Task TestMethod() =>
                await repo.GetByIdAsync<Product, string>((Guid?)null, p => p.Name);
            await Assert.ThrowsAsync<ArgumentNullException>(TestMethod);
        }

        [Fact]
        public async Task GetByIdAsync_Projected_ShouldThrow_WhenSelectExpressionIsNull()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetByIdAsync_Projected_ShouldThrow_WhenSelectExpressionIsNull));
            var repo = new Repository<TestDbContext>(context);

            // Act & Assert
            async Task TestMethod() =>
                await repo.GetByIdAsync<Product, string>(Guid.NewGuid(), null);
            await Assert.ThrowsAsync<ArgumentNullException>(TestMethod);
        }

        [Fact]
        public async Task GetByIdAsync_Projected_ShouldReturnNull_WhenEntityIsSoftDeleted()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetByIdAsync_Projected_ShouldReturnNull_WhenEntityIsSoftDeleted));
            var repo = new Repository<TestDbContext>(context);
            var product = new Product { Name = "TestProduct", IsDeleted = true };
            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();

            // Act
            var result = await repo.GetByIdAsync<Product, string>(product.Id, p => p.Name);

            // Assert
            Assert.Null(result);
        }
        #endregion

        #region GetByIdAsync (Object)
        [Fact]
        public async Task GetByIdAsync_Object_ShouldReturnEntity_WhenExists()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetByIdAsync_Object_ShouldReturnEntity_WhenExists));
            var repo = new Repository<TestDbContext>(context);
            var product = new Product { Name = "TestProduct" };
            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();

            // Act
            var result = await repo.GetByIdAsync<Product>( product.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(product.Name, result.Name);
        }

        [Fact]
        public async Task GetByIdAsync_Object_ShouldReturnNull_WhenNotExists()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetByIdAsync_Object_ShouldReturnNull_WhenNotExists));
            var repo = new Repository<TestDbContext>(context);

            // Act
            var result = await repo.GetByIdAsync<Product>( Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_Object_ShouldThrow_WhenIdIsNull()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetByIdAsync_Object_ShouldThrow_WhenIdIsNull));
            var repo = new Repository<TestDbContext>(context);

            // Act & Assert
            async Task TestMethod() => await repo.GetByIdAsync<Product>(null);
            await Assert.ThrowsAsync<ArgumentNullException>(TestMethod);
        }

        [Fact]
        public async Task GetByIdAsync_Object_WithIncludes_ShouldReturnEntityWithRelatedData()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetByIdAsync_Object_WithIncludes_ShouldReturnEntityWithRelatedData));
            var repo = new Repository<TestDbContext>(context);
            var customer = new Customer { Name = "TestCustomer" };
            var order = new Order { Description = "TestOrder", CustomerId = customer.Id };
            customer.Orders.Add(order);
            await context.Customers.AddAsync(customer);
            await context.Orders.AddAsync(order);
            await context.SaveChangesAsync();

            // Act
            var result = await repo.GetByIdAsync<Customer>(
                 customer.Id,
                q => q.Include(c => c.Orders));

            // Assert
            Assert.NotNull(result);
            Assert.Equal(customer.Name, result.Name);
            Assert.Single(result.Orders);
        }

        [Fact]
        public async Task GetByIdAsync_Object_AsNoTracking_ShouldReturnEntity()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetByIdAsync_Object_AsNoTracking_ShouldReturnEntity));
            var repo = new Repository<TestDbContext>(context);
            var product = new Product { Name = "TestProduct" };
            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();

            // Act
            var result = await repo.GetByIdAsync<Product>( product.Id, asNoTracking: true);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(product.Name, result.Name);
            Assert.Equal(EntityState.Detached, context.Entry(result).State);
        }
        #endregion

        #region GetByIdsAsync
        [Fact]
        public async Task GetByIdsAsync_ShouldReturnAllEntities()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetByIdsAsync_ShouldReturnAllEntities));
            var repo = new Repository<TestDbContext>(context);
            var products = new List<Product>
            {
                new Product { Name = "P1" },
                new Product { Name = "P2" },
                new Product { Name = "P3" }
            };
            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
            var ids = products.Select(p => (object)p.Id).ToList();

            // Act
            var results = await repo.GetByIdsAsync<Product>(ids);

            // Assert
            Assert.Equal(3, results.Count);
            Assert.All(products, p => Assert.Contains(results, r => r.Id == p.Id && r.Name == p.Name));
        }

        [Fact]
        public async Task GetByIdsAsync_ShouldReturnEmptyList_WhenIdsIsNull()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetByIdsAsync_ShouldReturnEmptyList_WhenIdsIsNull));
            var repo = new Repository<TestDbContext>(context);

            // Act
            var results = await repo.GetByIdsAsync<Product>(null);

            // Assert
            Assert.Empty(results);
        }

        [Fact]
        public async Task GetByIdsAsync_ShouldReturnEmptyList_WhenIdsIsEmpty()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetByIdsAsync_ShouldReturnEmptyList_WhenIdsIsEmpty));
            var repo = new Repository<TestDbContext>(context);

            // Act
            var results = await repo.GetByIdsAsync<Product>(new object[] { });

            // Assert
            Assert.Empty(results);
        }

        [Fact]
        public async Task GetByIdsAsync_ShouldExcludeSoftDeletedEntities()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetByIdsAsync_ShouldExcludeSoftDeletedEntities));
            var repo = new Repository<TestDbContext>(context);
            var products = new List<Product>
            {
                new Product { Name = "P1" },
                new Product { Name = "P2", IsDeleted = true },
                new Product { Name = "P3" }
            };
            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
            var ids = products.Select(p => (object)p.Id).ToList();

            // Act
            var results = await repo.GetByIdsAsync<Product>(ids);

            // Assert
            Assert.Equal(2, results.Count);
            Assert.DoesNotContain(results, r => r.Name == "P2");
        }

        [Fact]
        public async Task GetByIdsAsync_AsNoTracking_ShouldReturnDetachedEntities()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetByIdsAsync_AsNoTracking_ShouldReturnDetachedEntities));
            var repo = new Repository<TestDbContext>(context);
            var products = new List<Product> { new Product { Name = "P1" } };
            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
            var ids = products.Select(p => (object)p.Id).ToList();

            // Act
            var results = await repo.GetByIdsAsync<Product>(ids, asNoTracking: true);

            // Assert
            Assert.Single(results);
            Assert.Equal(EntityState.Detached, context.Entry(results.First()).State);
        }

        [Fact]
        public async Task GetByIdsAsync_ShouldCancel_WhenCancellationRequested()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetByIdsAsync_ShouldCancel_WhenCancellationRequested));
            var repo = new Repository<TestDbContext>(context);
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            async Task TestMethod() =>
                await repo.GetByIdsAsync<Product>(new object[] { Guid.NewGuid() }, cancellationToken: cts.Token);
            await Assert.ThrowsAsync<OperationCanceledException>(TestMethod);
        }
        #endregion

        #region GetOnlyAsync
        [Fact]
        public async Task GetOnlyAsync_ShouldReturnPropertyValue()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetOnlyAsync_ShouldReturnPropertyValue));
            var repo = new Repository<TestDbContext>(context);
            var product = new Product { Name = "SinglePropertyProduct" };
            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();

            // Act
            var name = await repo.GetOnlyAsync<Product, string>(
                product.Id,
                p => p.Name);

            // Assert
            Assert.Equal(product.Name, name);
        }

        [Fact]
        public async Task GetOnlyAsync_ShouldReturnDefault_WhenIdIsNull()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetOnlyAsync_ShouldReturnDefault_WhenIdIsNull));
            var repo = new Repository<TestDbContext>(context);

            // Act
            var name = await repo.GetOnlyAsync<Product, string>(
                null,
                p => p.Name);

            // Assert
            Assert.Null(name);
        }

        [Fact]
        public async Task GetOnlyAsync_ShouldReturnDefault_WhenEntityNotExists()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetOnlyAsync_ShouldReturnDefault_WhenEntityNotExists));
            var repo = new Repository<TestDbContext>(context);

            // Act
            var name = await repo.GetOnlyAsync<Product, string>(
                Guid.NewGuid(),
                p => p.Name);

            // Assert
            Assert.Null(name);
        }

        [Fact]
        public async Task GetOnlyAsync_ShouldReturnDefault_WhenEntityIsSoftDeleted()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetOnlyAsync_ShouldReturnDefault_WhenEntityIsSoftDeleted));
            var repo = new Repository<TestDbContext>(context);
            var product = new Product { Name = "TestProduct", IsDeleted = true };
            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();

            // Act
            var name = await repo.GetOnlyAsync<Product, string>(
                product.Id,
                p => p.Name);

            // Assert
            Assert.Null(name);
        }

        [Fact]
        public async Task GetOnlyAsync_ShouldCancel_WhenCancellationRequested()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetOnlyAsync_ShouldCancel_WhenCancellationRequested));
            var repo = new Repository<TestDbContext>(context);
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            async Task TestMethod() =>
                await repo.GetOnlyAsync<Product, string>(Guid.NewGuid(), p => p.Name, cts.Token);
            await Assert.ThrowsAsync<OperationCanceledException>(TestMethod);
        }
        #endregion

        #region TryGetByIdAsync
        [Fact]
        public async Task TryGetByIdAsync_ShouldReturnTrue_WhenEntityExists()
        {
            // Arrange
            using var context = GetDbContext(nameof(TryGetByIdAsync_ShouldReturnTrue_WhenEntityExists));
            var repo = new Repository<TestDbContext>(context);
            var product = new Product { Name = "SafeAccessProduct" };
            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();

            // Act
            var (success, entity) = await repo.TryGetByIdAsync<Product>(product.Id);

            // Assert
            Assert.True(success);
            Assert.NotNull(entity);
            Assert.Equal(product.Name, entity.Name);
        }

        [Fact]
        public async Task TryGetByIdAsync_ShouldReturnFalse_WhenEntityNotExists()
        {
            // Arrange
            using var context = GetDbContext(nameof(TryGetByIdAsync_ShouldReturnFalse_WhenEntityNotExists));
            var repo = new Repository<TestDbContext>(context);

            // Act
            var (success, entity) = await repo.TryGetByIdAsync<Product>(Guid.NewGuid());

            // Assert
            Assert.False(success);
            Assert.Null(entity);
        }

        [Fact]
        public async Task TryGetByIdAsync_ShouldReturnFalse_WhenEntityIsSoftDeleted()
        {
            // Arrange
            using var context = GetDbContext(nameof(TryGetByIdAsync_ShouldReturnFalse_WhenEntityIsSoftDeleted));
            var repo = new Repository<TestDbContext>(context);
            var product = new Product { Name = "TestProduct", IsDeleted = true };
            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();

            // Act
            var (success, entity) = await repo.TryGetByIdAsync<Product>(product.Id);

            // Assert
            Assert.False(success);
            Assert.Null(entity);
        }

        [Fact]
        public async Task TryGetByIdAsync_ShouldCancel_WhenCancellationRequested()
        {
            // Arrange
            using var context = GetDbContext(nameof(TryGetByIdAsync_ShouldCancel_WhenCancellationRequested));
            var repo = new Repository<TestDbContext>(context);
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            async Task TestMethod() =>
                await repo.TryGetByIdAsync<Product>(Guid.NewGuid(), cts.Token);
            await Assert.ThrowsAsync<OperationCanceledException>(TestMethod);
        }
        #endregion

        #region GetByIdOrDefaultAsync & FindByIdAsync
        [Fact]
        public async Task GetByIdOrDefaultAsync_ShouldReturnEntity_WhenExists()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetByIdOrDefaultAsync_ShouldReturnEntity_WhenExists));
            var repo = new Repository<TestDbContext>(context);
            var product = new Product { Name = "TestProduct" };
            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();

            // Act
            var result = await repo.GetByIdOrDefaultAsync<Product>(product.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(product.Name, result.Name);
        }

        [Fact]
        public async Task GetByIdOrDefaultAsync_ShouldReturnNull_WhenNotExists()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetByIdOrDefaultAsync_ShouldReturnNull_WhenNotExists));
            var repo = new Repository<TestDbContext>(context);

            // Act
            var result = await repo.GetByIdOrDefaultAsync<Product>(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task FindByIdAsync_ShouldReturnEntity_WhenExists()
        {
            // Arrange
            using var context = GetDbContext(nameof(FindByIdAsync_ShouldReturnEntity_WhenExists));
            var repo = new Repository<TestDbContext>(context);
            var product = new Product { Name = "TestProduct" };
            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();

            // Act
            var result = await repo.FindByIdAsync<Product>(product.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(product.Name, result.Name);
        }

        [Fact]
        public async Task FindByIdAsync_ShouldReturnNull_WhenNotExists()
        {
            // Arrange
            using var context = GetDbContext(nameof(FindByIdAsync_ShouldReturnNull_WhenNotExists));
            var repo = new Repository<TestDbContext>(context);

            // Act
            var result = await repo.FindByIdAsync<Product>(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }
        #endregion 
    }
}