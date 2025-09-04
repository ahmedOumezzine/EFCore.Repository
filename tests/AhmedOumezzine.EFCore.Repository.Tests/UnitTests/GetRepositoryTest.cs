using AhmedOumezzine.EFCore.Repository.Entities;
using AhmedOumezzine.EFCore.Repository.Repository;
using AhmedOumezzine.EFCore.Repository.Specification;
using AhmedOumezzine.EFCore.Repository.Tests.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AhmedOumezzine.EFCore.Repository.Tests.UnitTests
{
    public class GetRepositoryTests
    {
        private TestDbContext GetDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            return new TestDbContext(options);
        }

        #region GetAsync - By Condition
        [Fact]
        public async Task GetAsync_ByCondition_ShouldReturnCorrectEntity()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetAsync_ByCondition_ShouldReturnCorrectEntity));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddRangeAsync(
                new Product { Name = "A1" },
                new Product { Name = "B1" },
                new Product { Name = "Deleted", IsDeleted = true });
            await context.SaveChangesAsync();

            // Act
            var entity = await repo.GetAsync<Product>(p => p.Name == "A1");

            // Assert
            Assert.NotNull(entity);
            Assert.Equal("A1", entity.Name);
            Assert.False(entity.IsDeleted);
        }

        [Fact]
        public async Task GetAsync_ByCondition_ShouldReturnNull_WhenNoEntityMatches()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetAsync_ByCondition_ShouldReturnNull_WhenNoEntityMatches));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddAsync(new Product { Name = "B1" });
            await context.SaveChangesAsync();

            // Act
            var entity = await repo.GetAsync<Product>(p => p.Name == "A1");

            // Assert
            Assert.Null(entity);
        }

        [Fact]
        public async Task GetAsync_ByCondition_ShouldReturnNull_WhenEntityIsSoftDeleted()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetAsync_ByCondition_ShouldReturnNull_WhenEntityIsSoftDeleted));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddAsync(new Product { Name = "Deleted", IsDeleted = true });
            await context.SaveChangesAsync();

            // Act
            var entity = await repo.GetAsync<Product>(p => p.Name == "Deleted");

            // Assert
            Assert.Null(entity);
        }

        [Fact]
        public async Task GetAsync_ByCondition_WithAsNoTracking_ShouldReturnDetachedEntity()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetAsync_ByCondition_WithAsNoTracking_ShouldReturnDetachedEntity));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddAsync(new Product { Name = "A1" });
            await context.SaveChangesAsync();

            // Act
            var entity = await repo.GetAsync<Product>(p => p.Name == "A1", asNoTracking: true);

            // Assert
            Assert.NotNull(entity);
            Assert.Equal("A1", entity.Name);
            Assert.Equal(EntityState.Detached, context.Entry(entity).State);
        }

        [Fact]
        public async Task GetAsync_ByCondition_WithIncludes_ShouldReturnEntityWithRelatedData()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetAsync_ByCondition_WithIncludes_ShouldReturnEntityWithRelatedData));
            var repo = new Repository<TestDbContext>(context);
            var product = new Product { Name = "P1", Category = new Category { Name = "Electronics" } };
            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();

            // Act
            var entity = await repo.GetAsync<Product>(p => p.Name == "P1", q => q.Include(p => p.Category));

            // Assert
            Assert.NotNull(entity);
            Assert.Equal("P1", entity.Name);
            Assert.NotNull(entity.Category);
            Assert.Equal("Electronics", entity.Category.Name);
        }

        [Fact]
        public async Task GetAsync_ByCondition_WithIncludesAndAsNoTracking_ShouldReturnDetachedEntityWithRelatedData()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetAsync_ByCondition_WithIncludesAndAsNoTracking_ShouldReturnDetachedEntityWithRelatedData));
            var repo = new Repository<TestDbContext>(context);
            var product = new Product { Name = "P1", Category = new Category { Name = "Electronics" } };
            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();

            // Act
            var entity = await repo.GetAsync<Product>(p => p.Name == "P1", q => q.Include(p => p.Category), asNoTracking: true);

            // Assert
            Assert.NotNull(entity);
            Assert.Equal("P1", entity.Name);
            Assert.NotNull(entity.Category);
            Assert.Equal("Electronics", entity.Category.Name);
            Assert.Equal(EntityState.Detached, context.Entry(entity).State);
        }

        [Fact]
        public async Task GetAsync_ByCondition_ShouldReturnFirstNonDeleted_WhenConditionIsNull()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetAsync_ByCondition_ShouldReturnFirstNonDeleted_WhenConditionIsNull));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddRangeAsync(
                new Product { Name = "P1" },
                new Product { Name = "P2" });
            await context.SaveChangesAsync();

            // Act
            var entity = await repo.GetAsync<Product>((Expression<Func<Product, bool>>)null);

            // Assert
            Assert.NotNull(entity);
            Assert.Equal("P1", entity.Name);
        }

        [Fact]
        public async Task GetAsync_ByCondition_ShouldCancel_WhenCancellationRequested()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetAsync_ByCondition_ShouldCancel_WhenCancellationRequested));
            var repo = new Repository<TestDbContext>(context);
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            async Task TestMethod()
            {
                await repo.GetAsync<Product>(p => p.Name == "Test", cancellationToken: cts.Token);
            }
            await Assert.ThrowsAsync<OperationCanceledException>(TestMethod);
        }
        #endregion

        #region GetAsync - By Specification
        [Fact]
        public async Task GetAsync_BySpecification_ShouldReturnCorrectEntity()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetAsync_BySpecification_ShouldReturnCorrectEntity));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddRangeAsync(
                new Product { Name = "P1", IsActive = true },
                new Product { Name = "P2", IsActive = false });
            await context.SaveChangesAsync();
            var spec = new Specification<Product>();
            spec.Conditions.Add(p => p.IsActive);
            // Act
            var entity = await repo.GetAsync(spec);

            // Assert
            Assert.NotNull(entity);
            Assert.True(entity.IsActive);
            Assert.Equal("P1", entity.Name);
        }

        [Fact]
        public async Task GetAsync_BySpecification_ShouldThrow_WhenSpecificationIsNull()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetAsync_BySpecification_ShouldThrow_WhenSpecificationIsNull));
            var repo = new Repository<TestDbContext>(context);

            // Act & Assert
            async Task TestMethod()
            {
                await repo.GetAsync<Product>((Specification<Product>)null);
            }
            await Assert.ThrowsAsync<ArgumentNullException>(TestMethod);
        }

        [Fact]
        public async Task GetAsync_BySpecification_WithAsNoTracking_ShouldReturnDetachedEntity()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetAsync_BySpecification_WithAsNoTracking_ShouldReturnDetachedEntity));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddAsync(new Product { Name = "P1" });
            await context.SaveChangesAsync();
            var spec = new Specification<Product>();
            spec.Conditions.Add(p => p.Name == "P1");
            // Act
            var entity = await repo.GetAsync(spec, asNoTracking: true);

            // Assert
            Assert.NotNull(entity);
            Assert.Equal("P1", entity.Name);
            Assert.Equal(EntityState.Detached, context.Entry(entity).State);
        }

        [Fact]
        public async Task GetAsync_BySpecification_ShouldCancel_WhenCancellationRequested()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetAsync_BySpecification_ShouldCancel_WhenCancellationRequested));
            var repo = new Repository<TestDbContext>(context);
            var spec = new Specification<Product>();
            spec.Conditions.Add(p => p.Name == "Test");
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            async Task TestMethod()
            {
                await repo.GetAsync(spec, cancellationToken: cts.Token);
            }
            await Assert.ThrowsAsync<OperationCanceledException>(TestMethod);
        }
        #endregion

        #region GetAsync - Projection
        [Fact]
        public async Task GetAsync_Projection_ShouldReturnDto()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetAsync_Projection_ShouldReturnDto));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddAsync(new Product { Name = "P1", Price = 100 });
            await context.SaveChangesAsync();

            // Act
            var dto = await repo.GetAsync<Product, ProductDto>(
                p => p.Name == "P1",
                p => new ProductDto { Name = p.Name, Price = p.Price });

            // Assert
            Assert.NotNull(dto);
            Assert.Equal("P1", dto.Name);
            Assert.Equal(100, dto.Price);
        }

        [Fact]
        public async Task GetAsync_Projection_ShouldReturnNull_WhenNoEntityMatches()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetAsync_Projection_ShouldReturnNull_WhenNoEntityMatches));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddAsync(new Product { Name = "P2" });
            await context.SaveChangesAsync();

            // Act
            var dto = await repo.GetAsync<Product, ProductDto>(
                p => p.Name == "P1",
                p => new ProductDto { Name = p.Name, Price = p.Price });

            // Assert
            Assert.Null(dto);
        }

        [Fact]
        public async Task GetAsync_Projection_ShouldThrow_WhenSelectExpressionIsNull()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetAsync_Projection_ShouldThrow_WhenSelectExpressionIsNull));
            var repo = new Repository<TestDbContext>(context);

            // Act & Assert
            async Task TestMethod()
            {
                await repo.GetAsync<Product, ProductDto>(p => p.Name == "P1", null);
            }
            await Assert.ThrowsAsync<ArgumentNullException>(TestMethod);
        }

        [Fact]
        public async Task GetAsync_ProjectionBySpecification_ShouldReturnDto()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetAsync_ProjectionBySpecification_ShouldReturnDto));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddAsync(new Product { Name = "P1", Price = 100 });
            await context.SaveChangesAsync();
            var spec = new Specification<Product>();
            spec.Conditions.Add(p => p.Name == "P1");
            // Act
            var dto = await repo.GetAsync<Product, ProductDto>(
                spec,
                p => new ProductDto { Name = p.Name, Price = p.Price });

            // Assert
            Assert.NotNull(dto);
            Assert.Equal("P1", dto.Name);
            Assert.Equal(100, dto.Price);
        }

        [Fact]
        public async Task GetAsync_ProjectionBySpecification_ShouldThrow_WhenSpecificationIsNull()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetAsync_ProjectionBySpecification_ShouldThrow_WhenSpecificationIsNull));
            var repo = new Repository<TestDbContext>(context);

            // Act & Assert
            async Task TestMethod()
            {
                await repo.GetAsync<Product, ProductDto>(
                    (Specification<Product>)null,
                    p => new ProductDto { Name = p.Name, Price = p.Price });
            }
            await Assert.ThrowsAsync<ArgumentNullException>(TestMethod);
        }

        [Fact]
        public async Task GetAsync_ProjectionBySpecification_ShouldThrow_WhenSelectExpressionIsNull()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetAsync_ProjectionBySpecification_ShouldThrow_WhenSelectExpressionIsNull));
            var repo = new Repository<TestDbContext>(context);
            var spec = new Specification<Product>( );
            spec.Conditions.Add(p => p.Name == "P1");
            // Act & Assert
            async Task TestMethod()
            {
                await repo.GetAsync<Product, ProductDto>(spec, null);
            }
            await Assert.ThrowsAsync<ArgumentNullException>(TestMethod);
        }

        [Fact]
        public async Task GetAsync_Projection_ShouldCancel_WhenCancellationRequested()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetAsync_Projection_ShouldCancel_WhenCancellationRequested));
            var repo = new Repository<TestDbContext>(context);
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            async Task TestMethod()
            {
                await repo.GetAsync<Product, ProductDto>(
                    p => p.Name == "P1",
                    p => new ProductDto { Name = p.Name, Price = p.Price },
                    cts.Token);
            }
            await Assert.ThrowsAsync<OperationCanceledException>(TestMethod);
        }
        #endregion

        #region TryGetAsync
        [Fact]
        public async Task TryGetAsync_ShouldReturnSuccessAndEntity_WhenEntityExists()
        {
            // Arrange
            using var context = GetDbContext(nameof(TryGetAsync_ShouldReturnSuccessAndEntity_WhenEntityExists));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddAsync(new Product { Name = "Test" });
            await context.SaveChangesAsync();

            // Act
            var (success, entity) = await repo.TryGetAsync<Product>(p => p.Name == "Test");

            // Assert
            Assert.True(success);
            Assert.NotNull(entity);
            Assert.Equal("Test", entity.Name);
        }

        [Fact]
        public async Task TryGetAsync_ShouldReturnFalseAndNull_WhenNoEntityMatches()
        {
            // Arrange
            using var context = GetDbContext(nameof(TryGetAsync_ShouldReturnFalseAndNull_WhenNoEntityMatches));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddAsync(new Product { Name = "Other" });
            await context.SaveChangesAsync();

            // Act
            var (success, entity) = await repo.TryGetAsync<Product>(p => p.Name == "Test");

            // Assert
            Assert.False(success);
            Assert.Null(entity);
        }

        [Fact]
        public async Task TryGetAsync_ShouldReturnFalseAndNull_WhenEntityIsSoftDeleted()
        {
            // Arrange
            using var context = GetDbContext(nameof(TryGetAsync_ShouldReturnFalseAndNull_WhenEntityIsSoftDeleted));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddAsync(new Product { Name = "Test", IsDeleted = true });
            await context.SaveChangesAsync();

            // Act
            var (success, entity) = await repo.TryGetAsync<Product>(p => p.Name == "Test");

            // Assert
            Assert.False(success);
            Assert.Null(entity);
        }

        [Fact]
        public async Task TryGetAsync_ShouldReturnFirstNonDeleted_WhenConditionIsNull()
        {
            // Arrange
            using var context = GetDbContext(nameof(TryGetAsync_ShouldReturnFirstNonDeleted_WhenConditionIsNull));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddRangeAsync(
                new Product { Name = "P1" },
                new Product { Name = "P2" });
            await context.SaveChangesAsync();

            // Act
            var (success, entity) = await repo.TryGetAsync<Product>((Expression<Func<Product, bool>>)null);

            // Assert
            Assert.True(success);
            Assert.NotNull(entity);
            Assert.Equal("P1", entity.Name);
        }

        [Fact]
        public async Task TryGetAsync_ShouldCancel_WhenCancellationRequested()
        {
            // Arrange
            using var context = GetDbContext(nameof(TryGetAsync_ShouldCancel_WhenCancellationRequested));
            var repo = new Repository<TestDbContext>(context);
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            async Task TestMethod()
            {
                await repo.TryGetAsync<Product>(p => p.Name == "Test", cts.Token);
            }
            await Assert.ThrowsAsync<OperationCanceledException>(TestMethod);
        }
        #endregion

        #region GetAsyncOrDefault
        [Fact]
        public async Task GetAsyncOrDefault_ShouldReturnCorrectEntity()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetAsyncOrDefault_ShouldReturnCorrectEntity));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddAsync(new Product { Name = "P1" });
            await context.SaveChangesAsync();

            // Act
            var entity = await repo.GetAsyncOrDefault<Product>(p => p.Name == "P1");

            // Assert
            Assert.NotNull(entity);
            Assert.Equal("P1", entity.Name);
        }

        [Fact]
        public async Task GetAsyncOrDefault_ShouldReturnNull_WhenNoEntityMatches()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetAsyncOrDefault_ShouldReturnNull_WhenNoEntityMatches));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddAsync(new Product { Name = "P2" });
            await context.SaveChangesAsync();

            // Act
            var entity = await repo.GetAsyncOrDefault<Product>(p => p.Name == "P1");

            // Assert
            Assert.Null(entity);
        }

        [Fact]
        public async Task GetAsyncOrDefault_ShouldReturnFirstNonDeleted_WhenConditionIsNull()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetAsyncOrDefault_ShouldReturnFirstNonDeleted_WhenConditionIsNull));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddRangeAsync(
                new Product { Name = "P1" },
                new Product { Name = "P2" });
            await context.SaveChangesAsync();

            // Act
            var entity = await repo.GetAsyncOrDefault<Product>((Expression<Func<Product, bool>>)null);

            // Assert
            Assert.NotNull(entity);
            Assert.Equal("P1", entity.Name);
        }

        [Fact]
        public async Task GetAsyncOrDefault_ShouldCancel_WhenCancellationRequested()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetAsyncOrDefault_ShouldCancel_WhenCancellationRequested));
            var repo = new Repository<TestDbContext>(context);
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            async Task TestMethod()
            {
                await repo.GetAsyncOrDefault<Product>(p => p.Name == "Test", cts.Token);
            }
            await Assert.ThrowsAsync<OperationCanceledException>(TestMethod);
        }
        #endregion

        #region FindAsync
        [Fact]
        public async Task FindAsync_ShouldReturnCorrectEntity()
        {
            // Arrange
            using var context = GetDbContext(nameof(FindAsync_ShouldReturnCorrectEntity));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddAsync(new Product { Name = "P1" });
            await context.SaveChangesAsync();

            // Act
            var entity = await repo.FindAsync<Product>(p => p.Name == "P1");

            // Assert
            Assert.NotNull(entity);
            Assert.Equal("P1", entity.Name);
        }

        [Fact]
        public async Task FindAsync_ShouldReturnNull_WhenNoEntityMatches()
        {
            // Arrange
            using var context = GetDbContext(nameof(FindAsync_ShouldReturnNull_WhenNoEntityMatches));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddAsync(new Product { Name = "P2" });
            await context.SaveChangesAsync();

            // Act
            var entity = await repo.FindAsync<Product>(p => p.Name == "P1");

            // Assert
            Assert.Null(entity);
        }

        [Fact]
        public async Task FindAsync_ShouldReturnFirstNonDeleted_WhenConditionIsNull()
        {
            // Arrange
            using var context = GetDbContext(nameof(FindAsync_ShouldReturnFirstNonDeleted_WhenConditionIsNull));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddRangeAsync(
                new Product { Name = "P1" },
                new Product { Name = "P2" });
            await context.SaveChangesAsync();

            // Act
            var entity = await repo.FindAsync<Product>((Expression<Func<Product, bool>>)null);

            // Assert
            Assert.NotNull(entity);
            Assert.Equal("P1", entity.Name);
        }

        [Fact]
        public async Task FindAsync_ShouldCancel_WhenCancellationRequested()
        {
            // Arrange
            using var context = GetDbContext(nameof(FindAsync_ShouldCancel_WhenCancellationRequested));
            var repo = new Repository<TestDbContext>(context);
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            async Task TestMethod()
            {
                await repo.FindAsync<Product>(p => p.Name == "Test", cts.Token);
            }
            await Assert.ThrowsAsync<OperationCanceledException>(TestMethod);
        }
        #endregion

        #region GetFirstOrThrowAsync
        [Fact]
        public async Task GetFirstOrThrowAsync_ShouldReturnEntity_WhenFound()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetFirstOrThrowAsync_ShouldReturnEntity_WhenFound));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddAsync(new Product { Name = "P1" });
            await context.SaveChangesAsync();

            // Act
            var entity = await repo.GetFirstOrThrowAsync<Product>(p => p.Name == "P1");

            // Assert
            Assert.NotNull(entity);
            Assert.Equal("P1", entity.Name);
        }

        [Fact]
        public async Task GetFirstOrThrowAsync_ShouldThrow_WhenNotFound()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetFirstOrThrowAsync_ShouldThrow_WhenNotFound));
            var repo = new Repository<TestDbContext>(context);

            // Act & Assert
            async Task TestMethod()
            {
                await repo.GetFirstOrThrowAsync<Product>(p => p.Name == "NonExistent", "Entity not found");
            }
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(TestMethod);
            Assert.Equal("Entity not found", exception.Message);
        }

        [Fact]
        public async Task GetFirstOrThrowAsync_ShouldThrowDefaultMessage_WhenErrorMessageIsNull()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetFirstOrThrowAsync_ShouldThrowDefaultMessage_WhenErrorMessageIsNull));
            var repo = new Repository<TestDbContext>(context);

            // Act & Assert
            async Task TestMethod()
            {
                await repo.GetFirstOrThrowAsync<Product>(p => p.Name == "NonExistent");
            }
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(TestMethod);
            Assert.Equal($"Entity of type {typeof(Product).Name} not found for the given condition.", exception.Message);
        }

        [Fact]
        public async Task GetFirstOrThrowAsync_ShouldCancel_WhenCancellationRequested()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetFirstOrThrowAsync_ShouldCancel_WhenCancellationRequested));
            var repo = new Repository<TestDbContext>(context);
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            async Task TestMethod()
            {
                await repo.GetFirstOrThrowAsync<Product>(p => p.Name == "Test", cancellationToken: cts.Token);
            }
            await Assert.ThrowsAsync<OperationCanceledException>(TestMethod);
        }
        #endregion

        #region ExistsAndFetchAsync
        [Fact]
        public async Task ExistsAndFetchAsync_ShouldReturnCorrectTuple_WhenEntityExists()
        {
            // Arrange
            using var context = GetDbContext(nameof(ExistsAndFetchAsync_ShouldReturnCorrectTuple_WhenEntityExists));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddAsync(new Product { Name = "X" });
            await context.SaveChangesAsync();

            // Act
            var (exists, entity) = await repo.ExistsAndFetchAsync<Product>(p => p.Name == "X");

            // Assert
            Assert.True(exists);
            Assert.NotNull(entity);
            Assert.Equal("X", entity.Name);
        }

        [Fact]
        public async Task ExistsAndFetchAsync_ShouldReturnFalseAndNull_WhenNoEntityMatches()
        {
            // Arrange
            using var context = GetDbContext(nameof(ExistsAndFetchAsync_ShouldReturnFalseAndNull_WhenNoEntityMatches));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddAsync(new Product { Name = "Y" });
            await context.SaveChangesAsync();

            // Act
            var (exists, entity) = await repo.ExistsAndFetchAsync<Product>(p => p.Name == "X");

            // Assert
            Assert.False(exists);
            Assert.Null(entity);
        }

        [Fact]
        public async Task ExistsAndFetchAsync_ShouldReturnFirstNonDeleted_WhenConditionIsNull()
        {
            // Arrange
            using var context = GetDbContext(nameof(ExistsAndFetchAsync_ShouldReturnFirstNonDeleted_WhenConditionIsNull));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddRangeAsync(
                new Product { Name = "P1" },
                new Product { Name = "P2" });
            await context.SaveChangesAsync();

            // Act
            var (exists, entity) = await repo.ExistsAndFetchAsync<Product>((Expression<Func<Product, bool>>)null);

            // Assert
            Assert.True(exists);
            Assert.NotNull(entity);
            Assert.Equal("P1", entity.Name);
        }

        [Fact]
        public async Task ExistsAndFetchAsync_ShouldCancel_WhenCancellationRequested()
        {
            // Arrange
            using var context = GetDbContext(nameof(ExistsAndFetchAsync_ShouldCancel_WhenCancellationRequested));
            var repo = new Repository<TestDbContext>(context);
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            async Task TestMethod()
            {
                await repo.ExistsAndFetchAsync<Product>(p => p.Name == "Test", cts.Token);
            }
            await Assert.ThrowsAsync<OperationCanceledException>(TestMethod);
        }
        #endregion

        #region GetOnlyAsync
        [Fact]
        public async Task GetOnlyAsync_ShouldReturnSingleProperty()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetOnlyAsync_ShouldReturnSingleProperty));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddAsync(new Product { Name = "TestName" });
            await context.SaveChangesAsync();

            // Act
            var name = await repo.GetOnlyAsync<Product, string>(p => p.Name == "TestName", p => p.Name);

            // Assert
            Assert.Equal("TestName", name);
        }

        [Fact]
        public async Task GetOnlyAsync_ShouldReturnDefault_WhenNoEntityMatches()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetOnlyAsync_ShouldReturnDefault_WhenNoEntityMatches));
            var repo = new Repository<TestDbContext>(context);
            await context.Products.AddAsync(new Product { Name = "Other" });
            await context.SaveChangesAsync();

            // Act
            var name = await repo.GetOnlyAsync<Product, string>(p => p.Name == "TestName", p => p.Name);

            // Assert
            Assert.Null(name);
        }

        [Fact]
        public async Task GetOnlyAsync_ShouldThrow_WhenConditionIsNull()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetOnlyAsync_ShouldThrow_WhenConditionIsNull));
            var repo = new Repository<TestDbContext>(context);

            // Act & Assert
            async Task TestMethod()
            {
                await repo.GetOnlyAsync<Product, string>(null, p => p.Name);
            }
            await Assert.ThrowsAsync<ArgumentNullException>(TestMethod);
        }

        [Fact]
        public async Task GetOnlyAsync_ShouldThrow_WhenPropertySelectorIsNull()
        {
            // Arrange
            using var context = GetDbContext(nameof(GetOnlyAsync_ShouldThrow_WhenPropertySelectorIsNull));
            var repo = new Repository<TestDbContext>(context);

            // Act & Assert
            async Task TestMethod()
            {
                await repo.GetOnlyAsync<Product, string>(p => p.Name == "Test", null);
            }
            await Assert.ThrowsAsync<ArgumentNullException>(TestMethod);
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
            async Task TestMethod()
            {
                await repo.GetOnlyAsync<Product, string>(p => p.Name == "Test", p => p.Name, cts.Token);
            }
            await Assert.ThrowsAsync<OperationCanceledException>(TestMethod);
        }
        #endregion 
    }
}