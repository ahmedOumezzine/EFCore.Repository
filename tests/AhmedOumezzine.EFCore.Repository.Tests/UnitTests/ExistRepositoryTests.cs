
using AhmedOumezzine.EFCore.Repository.Entities;
using AhmedOumezzine.EFCore.Repository.Repository;
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
    public class RepositoryExistsTests
    {
        private TestDbContext GetDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            return new TestDbContext(options);
        }

        #region ExistsByIdAsync (Guid)
        [Fact]
        public async Task ExistsByIdAsync_Guid_ShouldReturnTrue_WhenEntityExists()
        {
            // Arrange
            using var context = GetDbContext(nameof(ExistsByIdAsync_Guid_ShouldReturnTrue_WhenEntityExists));
            var repo = new Repository<TestDbContext>(context);
            var product = new Product { Name = "TestProduct" };
            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();

            // Act
            var exists = await repo.ExistsByIdAsync<Product>(product.Id);

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsByIdAsync_Guid_ShouldReturnFalse_WhenEntityDoesNotExist()
        {
            // Arrange
            using var context = GetDbContext(nameof(ExistsByIdAsync_Guid_ShouldReturnFalse_WhenEntityDoesNotExist));
            var repo = new Repository<TestDbContext>(context);

            // Act
            var exists = await repo.ExistsByIdAsync<Product>(Guid.NewGuid());

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public async Task ExistsByIdAsync_Guid_ShouldReturnFalse_WhenIdIsNull()
        {
            // Arrange
            using var context = GetDbContext(nameof(ExistsByIdAsync_Guid_ShouldReturnFalse_WhenIdIsNull));
            var repo = new Repository<TestDbContext>(context);

            // Act
            var exists = await repo.ExistsByIdAsync<Product>((Guid?)null);

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public async Task ExistsByIdAsync_Guid_ShouldReturnFalse_WhenEntityIsSoftDeleted()
        {
            // Arrange
            using var context = GetDbContext(nameof(ExistsByIdAsync_Guid_ShouldReturnFalse_WhenEntityIsSoftDeleted));
            var repo = new Repository<TestDbContext>(context);
            var product = new Product { Name = "TestProduct", IsDeleted = true };
            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();

            // Act
            var exists = await repo.ExistsByIdAsync<Product>(product.Id);

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public async Task ExistsByIdAsync_Guid_ShouldCancel_WhenCancellationRequested()
        {
            // Arrange
            using var context = GetDbContext(nameof(ExistsByIdAsync_Guid_ShouldCancel_WhenCancellationRequested));
            var repo = new Repository<TestDbContext>(context);
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            async Task TestMethod() => await repo.ExistsByIdAsync<Product>(Guid.NewGuid(), cts.Token);
           await Assert.ThrowsAsync<OperationCanceledException>(TestMethod);
        }
        #endregion

        #region ExistsByIdAsync (object)
        [Fact]
        public async Task ExistsByIdAsync_Object_ShouldReturnTrue_WhenEntityExists()
        {
            // Arrange
            using var context = GetDbContext(nameof(ExistsByIdAsync_Object_ShouldReturnTrue_WhenEntityExists));
            var repo = new Repository<TestDbContext>(context);
            var product = new Product { Name = "TestProduct" };
            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();

            // Act
            var exists = await repo.ExistsByIdAsync<Product>(product.Id);

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsByIdAsync_Object_ShouldReturnFalse_WhenEntityDoesNotExist()
        {
            // Arrange
            using var context = GetDbContext(nameof(ExistsByIdAsync_Object_ShouldReturnFalse_WhenEntityDoesNotExist));
            var repo = new Repository<TestDbContext>(context);

            // Act
            var exists = await repo.ExistsByIdAsync<Product>(Guid.NewGuid());

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public async Task ExistsByIdAsync_Object_ShouldReturnFalse_WhenKeyValueIsNull()
        {
            // Arrange
            using var context = GetDbContext(nameof(ExistsByIdAsync_Object_ShouldReturnFalse_WhenKeyValueIsNull));
            var repo = new Repository<TestDbContext>(context);

            // Act
            var exists = await repo.ExistsByIdAsync<Product>((object)null);

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public async Task ExistsByIdAsync_Object_ShouldReturnFalse_WhenEntityIsSoftDeleted()
        {
            // Arrange
            using var context = GetDbContext(nameof(ExistsByIdAsync_Object_ShouldReturnFalse_WhenEntityIsSoftDeleted));
            var repo = new Repository<TestDbContext>(context);
            var product = new Product { Name = "TestProduct", IsDeleted = true };
            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();

            // Act
            var exists = await repo.ExistsByIdAsync<Product>(product.Id);

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public async Task ExistsByIdAsync_Object_ShouldThrow_WhenNoPrimaryKeyDefined()
        {
            // Arrange
            using var context = GetDbContext(nameof(ExistsByIdAsync_Object_ShouldThrow_WhenNoPrimaryKeyDefined));
            var repo = new Repository<TestDbContext>(context);

            // Act & Assert
            async Task TestMethod() =>
                await repo.ExistsByIdAsync<NoPrimaryKeyEntity>(Guid.NewGuid());
            await Assert.ThrowsAsync<ArgumentException>(TestMethod);
        }

        [Fact]
        public async Task ExistsByIdAsync_Object_ShouldCancel_WhenCancellationRequested()
        {
            // Arrange
            using var context = GetDbContext(nameof(ExistsByIdAsync_Object_ShouldCancel_WhenCancellationRequested));
            var repo = new Repository<TestDbContext>(context);
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            async Task TestMethod() =>
                await repo.ExistsByIdAsync<Product>(Guid.NewGuid(), cts.Token);
            await Assert.ThrowsAsync<OperationCanceledException>(TestMethod);
        }
        #endregion

        #region ExistsAsync
        [Fact]
        public async Task ExistsAsync_ShouldReturnTrue_WhenAnyEntityExists()
        {
            // Arrange
            using var context = GetDbContext(nameof(ExistsAsync_ShouldReturnTrue_WhenAnyEntityExists));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddAsync(new Product { Name = "Prod1" });
            await context.SaveChangesAsync();

            // Act
            var exists = await repo.ExistsAsync<Product>();

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsAsync_ShouldReturnFalse_WhenNoEntityExists()
        {
            // Arrange
            using var context = GetDbContext(nameof(ExistsAsync_ShouldReturnFalse_WhenNoEntityExists));
            var repo = new Repository<TestDbContext>(context);

            // Act
            var exists = await repo.ExistsAsync<Product>();

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public async Task ExistsAsync_WithCondition_ShouldReturnTrue_WhenConditionMatches()
        {
            // Arrange
            using var context = GetDbContext(nameof(ExistsAsync_WithCondition_ShouldReturnTrue_WhenConditionMatches));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddRangeAsync(
                new Product { Name = "Apple" },
                new Product { Name = "Banana" });
            await context.SaveChangesAsync();

            // Act
            var exists = await repo.ExistsAsync<Product>(p => p.Name == "Apple");

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsAsync_WithCondition_ShouldReturnFalse_WhenConditionDoesNotMatch()
        {
            // Arrange
            using var context = GetDbContext(nameof(ExistsAsync_WithCondition_ShouldReturnFalse_WhenConditionDoesNotMatch));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddAsync(new Product { Name = "Banana" });
            await context.SaveChangesAsync();

            // Act
            var exists = await repo.ExistsAsync<Product>(p => p.Name == "Apple");

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public async Task ExistsAsync_WithCondition_ShouldThrow_WhenConditionIsNull()
        {
            // Arrange
            using var context = GetDbContext(nameof(ExistsAsync_WithCondition_ShouldThrow_WhenConditionIsNull));
            var repo = new Repository<TestDbContext>(context);

            // Act & Assert
            async Task TestMethod() =>
                await repo.ExistsAsync<Product>((Expression<Func<Product, bool>>)null);
            await Assert.ThrowsAsync<ArgumentNullException>(TestMethod);
        }

        [Fact]
        public async Task ExistsAsync_WithCondition_ShouldReturnFalse_WhenEntityIsSoftDeleted()
        {
            // Arrange
            using var context = GetDbContext(nameof(ExistsAsync_WithCondition_ShouldReturnFalse_WhenEntityIsSoftDeleted));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddAsync(new Product { Name = "Apple", IsDeleted = true });
            await context.SaveChangesAsync();

            // Act
            var exists = await repo.ExistsAsync<Product>(p => p.Name == "Apple");

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public async Task ExistsAsync_WithCancellationToken_ShouldCancel()
        {
            // Arrange
            using var context = GetDbContext(nameof(ExistsAsync_WithCancellationToken_ShouldCancel));
            var repo = new Repository<TestDbContext>(context);
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            async Task TestMethod() =>
                await repo.ExistsAsync<Product>(cts.Token);
            await Assert.ThrowsAsync<OperationCanceledException>(TestMethod);
        }
        #endregion

        #region ExistsByCompositeKeyAsync
        [Fact]
        public async Task ExistsByCompositeKeyAsync_ShouldReturnTrue_WhenEntityExists()
        {
            // Arrange
            using var context = GetDbContext(nameof(ExistsByCompositeKeyAsync_ShouldReturnTrue_WhenEntityExists));
            var repo = new Repository<TestDbContext>(context);
            var product = new Product { Name = "CompositeTest" };
            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();

            // Act
            var exists = await repo.ExistsByCompositeKeyAsync<Product>(new object[] { product.Id });

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsByCompositeKeyAsync_ShouldReturnFalse_WhenEntityDoesNotExist()
        {
            // Arrange
            using var context = GetDbContext(nameof(ExistsByCompositeKeyAsync_ShouldReturnFalse_WhenEntityDoesNotExist));
            var repo = new Repository<TestDbContext>(context);

            // Act
            var exists = await repo.ExistsByCompositeKeyAsync<Product>(new object[] { Guid.NewGuid() });

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public async Task ExistsByCompositeKeyAsync_ShouldReturnFalse_WhenKeyValuesIsNull()
        {
            // Arrange
            using var context = GetDbContext(nameof(ExistsByCompositeKeyAsync_ShouldReturnFalse_WhenKeyValuesIsNull));
            var repo = new Repository<TestDbContext>(context);

            // Act
            var exists = await repo.ExistsByCompositeKeyAsync<Product>((object[])null);

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public async Task ExistsByCompositeKeyAsync_ShouldReturnFalse_WhenKeyValuesIsEmpty()
        {
            // Arrange
            using var context = GetDbContext(nameof(ExistsByCompositeKeyAsync_ShouldReturnFalse_WhenKeyValuesIsEmpty));
            var repo = new Repository<TestDbContext>(context);

            // Act
            var exists = await repo.ExistsByCompositeKeyAsync<Product>(new object[] { });

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public async Task ExistsByCompositeKeyAsync_ShouldReturnFalse_WhenEntityIsSoftDeleted()
        {
            // Arrange
            using var context = GetDbContext(nameof(ExistsByCompositeKeyAsync_ShouldReturnFalse_WhenEntityIsSoftDeleted));
            var repo = new Repository<TestDbContext>(context);
            var product = new Product { Name = "CompositeTest", IsDeleted = true };
            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();

            // Act
            var exists = await repo.ExistsByCompositeKeyAsync<Product>(new object[] { product.Id });

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public async Task ExistsByCompositeKeyAsync_ShouldCancel_WhenCancellationRequested()
        {
            // Arrange
            using var context = GetDbContext(nameof(ExistsByCompositeKeyAsync_ShouldCancel_WhenCancellationRequested));
            var repo = new Repository<TestDbContext>(context);
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            async Task TestMethod() =>
                await repo.ExistsByCompositeKeyAsync<Product>(new object[] { Guid.NewGuid() }, cts.Token);
            await Assert.ThrowsAsync<OperationCanceledException>(TestMethod);
        }
        #endregion

        #region CountAsync
        [Fact]
        public async Task CountAsync_ShouldReturnCorrectNumber()
        {
            // Arrange
            using var context = GetDbContext(nameof(CountAsync_ShouldReturnCorrectNumber));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddRangeAsync(
                new Product { Name = "A" },
                new Product { Name = "B", IsDeleted = true },
                new Product { Name = "C" });
            await context.SaveChangesAsync();

            // Act
            var count = await repo.CountAsync<Product>();

            // Assert
            Assert.Equal(2, count); // Exclut l'entité supprimée
        }

        [Fact]
        public async Task CountAsync_ShouldReturnZero_WhenNoEntitiesExist()
        {
            // Arrange
            using var context = GetDbContext(nameof(CountAsync_ShouldReturnZero_WhenNoEntitiesExist));
            var repo = new Repository<TestDbContext>(context);

            // Act
            var count = await repo.CountAsync<Product>();

            // Assert
            Assert.Equal(0, count);
        }

        [Fact]
        public async Task CountAsync_WithCondition_ShouldReturnCorrectNumber()
        {
            // Arrange
            using var context = GetDbContext(nameof(CountAsync_WithCondition_ShouldReturnCorrectNumber));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddRangeAsync(
                new Product { Name = "Old", CreatedOnUtc = DateTime.UtcNow.AddYears(-2) },
                new Product { Name = "New", CreatedOnUtc = DateTime.UtcNow });
            await context.SaveChangesAsync();

            // Act
            var count = await repo.CountAsync<Product>(p => p.CreatedOnUtc < DateTime.UtcNow.AddYears(-1));

            // Assert
            Assert.Equal(1, count);
        }

        [Fact]
        public async Task CountAsync_WithCondition_ShouldThrow_WhenConditionIsNull()
        {
            // Arrange
            using var context = GetDbContext(nameof(CountAsync_WithCondition_ShouldThrow_WhenConditionIsNull));
            var repo = new Repository<TestDbContext>(context);

            // Act & Assert
            async Task TestMethod() =>
                await repo.CountAsync<Product>((Expression<Func<Product, bool>>)null);
            await Assert.ThrowsAsync<ArgumentNullException>(TestMethod);
        }

        [Fact]
        public async Task CountAsync_WithCondition_ShouldReturnZero_WhenEntitiesAreSoftDeleted()
        {
            // Arrange
            using var context = GetDbContext(nameof(CountAsync_WithCondition_ShouldReturnZero_WhenEntitiesAreSoftDeleted));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddAsync(new Product { Name = "Old", CreatedOnUtc = DateTime.UtcNow.AddYears(-2), IsDeleted = true });
            await context.SaveChangesAsync();

            // Act
            var count = await repo.CountAsync<Product>(p => p.CreatedOnUtc < DateTime.UtcNow.AddYears(-1));

            // Assert
            Assert.Equal(0, count);
        }

        [Fact]
        public async Task CountAsync_WithCancellationToken_ShouldCancel()
        {
            // Arrange
            using var context = GetDbContext(nameof(CountAsync_WithCancellationToken_ShouldCancel));
            var repo = new Repository<TestDbContext>(context);
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            async Task TestMethod() =>
                await repo.CountAsync<Product>(cts.Token);
            await Assert.ThrowsAsync<OperationCanceledException>(TestMethod);
        }
        #endregion

        #region IsDeletedAsync
        [Fact]
        public async Task IsDeletedAsync_ShouldReturnTrue_WhenEntityIsSoftDeleted()
        {
            // Arrange
            using var context = GetDbContext(nameof(IsDeletedAsync_ShouldReturnTrue_WhenEntityIsSoftDeleted));
            var repo = new Repository<TestDbContext>(context);
            var product = new Product { Name = "TestProduct", IsDeleted = true };
            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();

            // Act
            var isDeleted = await repo.IsDeletedAsync(product);

            // Assert
            Assert.True(isDeleted);
        }

        [Fact]
        public async Task IsDeletedAsync_ShouldReturnFalse_WhenEntityIsNotSoftDeleted()
        {
            // Arrange
            using var context = GetDbContext(nameof(IsDeletedAsync_ShouldReturnFalse_WhenEntityIsNotSoftDeleted));
            var repo = new Repository<TestDbContext>(context);
            var product = new Product { Name = "TestProduct", IsDeleted = false };
            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();

            // Act
            var isDeleted = await repo.IsDeletedAsync(product);

            // Assert
            Assert.False(isDeleted);
        }

        [Fact]
        public async Task IsDeletedAsync_ShouldThrow_WhenEntityIsNull()
        {
            // Arrange
            using var context = GetDbContext(nameof(IsDeletedAsync_ShouldThrow_WhenEntityIsNull));
            var repo = new Repository<TestDbContext>(context);

            // Act & Assert
            async Task TestMethod() =>
                await repo.IsDeletedAsync<Product>(null);
            await Assert.ThrowsAsync<ArgumentNullException>(TestMethod);
        }

        [Fact]
        public async Task IsDeletedAsync_ShouldReturnFalse_WhenEntityDoesNotExist()
        {
            // Arrange
            using var context = GetDbContext(nameof(IsDeletedAsync_ShouldReturnFalse_WhenEntityDoesNotExist));
            var repo = new Repository<TestDbContext>(context);
            var product = new Product { Id = Guid.NewGuid(), Name = "NonExistent" };

            // Act
            var isDeleted = await repo.IsDeletedAsync(product);

            // Assert
            Assert.False(isDeleted);
        }

        [Fact]
        public async Task IsDeletedAsync_ShouldCancel_WhenCancellationRequested()
        {
            // Arrange
            using var context = GetDbContext(nameof(IsDeletedAsync_ShouldCancel_WhenCancellationRequested));
            var repo = new Repository<TestDbContext>(context);
            var product = new Product { Name = "TestProduct" };
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            async Task TestMethod() =>
                await repo.IsDeletedAsync(product, cts.Token);
            await Assert.ThrowsAsync<OperationCanceledException>(TestMethod);
        }
        #endregion
         
    }
}