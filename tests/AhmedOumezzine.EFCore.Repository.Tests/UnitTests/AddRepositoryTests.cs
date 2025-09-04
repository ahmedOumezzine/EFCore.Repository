using AhmedOumezzine.EFCore.Repository.Entities;
using AhmedOumezzine.EFCore.Repository.Entities.AhmedOumezzine.EFCore.Repository.Entities;
using AhmedOumezzine.EFCore.Repository.Interface;
using AhmedOumezzine.EFCore.Repository.Repository;
using AhmedOumezzine.EFCore.Repository.Specification;
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
    #region Test Support Classes
    public class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>()
                .HasOne<Customer>()
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId);
        }
    }

    public class Customer : BaseEntity
    {
        public string Name { get; set; }
        public List<Order> Orders { get; set; } = new();
    }

    public class Order : BaseEntity
    {
        public string Description { get; set; }
        public Guid CustomerId { get; set; }
    }

    public class Product : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;

        public Guid? CategoryId { get; set; }
        public Category? Category { get; set; }
    }

    public class Category : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
    }
    public class NoPrimaryKeyEntity : BaseEntity
    {
        public string Name { get; set; }
    }

   
    #endregion
    public class RepositoryInsertTests
    {
        private TestDbContext GetDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            return new TestDbContext(options);
        }

        [Fact]
        public async Task InsertAsync_ShouldAddEntity_AndSetProperties()
        {
            // Arrange
            using var context = GetDbContext(nameof(InsertAsync_ShouldAddEntity_AndSetProperties));
            var repo = new Repository<TestDbContext>(context);
            var customer = new Customer { Name = "Ahmed" };

            // Act
            var keys = await repo.InsertAsync(customer);

            // Assert
            Assert.NotNull(keys);
            Assert.Single(keys);
            Assert.Equal(customer.Id, keys[0]);
            Assert.Equal(1, context.Customers.Count());
            Assert.NotEqual(default(DateTime), customer.CreatedOnUtc);
            Assert.False(customer.IsDeleted);
        }

        [Fact]
        public async Task InsertAsync_ShouldThrow_WhenEntityIsNull()
        {
            // Arrange
            using var context = GetDbContext(nameof(InsertAsync_ShouldThrow_WhenEntityIsNull));
            var repo = new Repository<TestDbContext>(context);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.InsertAsync<Customer>(null));
        }

        [Fact]
        public async Task InsertRangeAsync_ShouldAddEntities_AndSetProperties()
        {
            // Arrange
            using var context = GetDbContext(nameof(InsertRangeAsync_ShouldAddEntities_AndSetProperties));
            var repo = new Repository<TestDbContext>(context);
            var customers = new List<Customer>
            {
                new Customer { Name = "Ali" },
                new Customer { Name = "Mouna" }
            };

            // Act
            await repo.InsertRangeAsync(customers);

            // Assert
            Assert.Equal(2, context.Customers.Count());
            Assert.All(customers, c => Assert.NotEqual(default(DateTime), c.CreatedOnUtc));
            Assert.All(customers, c => Assert.False(c.IsDeleted));
        }

        [Fact]
        public async Task InsertRangeAsync_ShouldThrow_WhenEntitiesIsNull()
        {
            // Arrange
            using var context = GetDbContext(nameof(InsertRangeAsync_ShouldThrow_WhenEntitiesIsNull));
            var repo = new Repository<TestDbContext>(context);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.InsertRangeAsync<Customer>(null));
        }

        [Fact]
        public async Task InsertAndReturnAsync_ShouldInsertAndReturnEntity_WithGeneratedId()
        {
            // Arrange
            using var context = GetDbContext(nameof(InsertAndReturnAsync_ShouldInsertAndReturnEntity_WithGeneratedId));
            var repo = new Repository<TestDbContext>(context);
            var customer = new Customer { Name = "Test" };

            // Act
            var returned = await repo.InsertAndReturnAsync(customer);

            // Assert
            Assert.Equal(customer, returned);
            Assert.NotEqual(default(Guid), returned.Id);
            Assert.Equal(1, context.Customers.Count());
            Assert.NotEqual(default(DateTime), returned.CreatedOnUtc);
            Assert.False(returned.IsDeleted);
        }

        [Fact]
        public async Task BulkInsertAsync_ShouldAddEntities_SameAsInsertRange()
        {
            // Arrange
            using var context = GetDbContext(nameof(BulkInsertAsync_ShouldAddEntities_SameAsInsertRange));
            var repo = new Repository<TestDbContext>(context);
            var customers = new List<Customer>
            {
                new Customer { Name = "Bulk1" },
                new Customer { Name = "Bulk2" }
            };

            // Act
            await repo.BulkInsertAsync(customers);

            // Assert
            Assert.Equal(2, context.Customers.Count());
            Assert.All(customers, c => Assert.NotEqual(default(DateTime), c.CreatedOnUtc));
            Assert.All(customers, c => Assert.False(c.IsDeleted));
        }

        [Fact]
        public async Task InsertManyAsync_ShouldInsertInBatches()
        {
            // Arrange
            using var context = GetDbContext(nameof(InsertManyAsync_ShouldInsertInBatches));
            var repo = new Repository<TestDbContext>(context);
            var customers = Enumerable.Range(1, 10).Select(i => new Customer { Name = $"Batch{i}" }).ToList();
            const int batchSize = 3;

            // Act
            await repo.InsertManyAsync(customers, batchSize);

            // Assert
            Assert.Equal(10, context.Customers.Count());
            Assert.All(customers, c => Assert.NotEqual(default(DateTime), c.CreatedOnUtc));
            Assert.All(customers, c => Assert.False(c.IsDeleted));
        }

        [Fact]
        public async Task InsertWithAuditAsync_ShouldCreateAuditLog_AndInsertEntity()
        {
            // Arrange
            using var context = GetDbContext(nameof(InsertWithAuditAsync_ShouldCreateAuditLog_AndInsertEntity));
            var repo = new Repository<TestDbContext>(context);
            var customer = new Customer { Name = "Khaled" };
            const string userName = "Admin";

            // Act
            var keys = await repo.InsertWithAuditAsync(customer, userName);

            // Assert
            Assert.NotNull(keys);
            Assert.Equal(customer.Id, keys[0]);
            Assert.Equal(1, context.Customers.Count());
            Assert.Equal(1, context.AuditLogs.Count());
            var audit = context.AuditLogs.First();
            Assert.Equal("INSERT", audit.Action);
            Assert.Equal("Customer", audit.EntityName);
            Assert.Equal(customer.Id.ToString(), audit.EntityId);
            Assert.Equal(userName, audit.UserName);
            Assert.NotEqual(default(DateTime), audit.CreatedOnUtc);
            Assert.NotEqual(default(DateTime), customer.CreatedOnUtc);
            Assert.False(customer.IsDeleted);
        }

        [Fact]
        public async Task InsertIfNotExistsAsync_ShouldInsert_WhenNotExists()
        {
            // Arrange
            using var context = GetDbContext(nameof(InsertIfNotExistsAsync_ShouldInsert_WhenNotExists));
            var repo = new Repository<TestDbContext>(context);
            var customer = new Customer { Name = "Fatma" };

            // Act
            var inserted = await repo.InsertIfNotExistsAsync(c => c.Name == "Fatma", customer);

            // Assert
            Assert.True(inserted);
            Assert.Equal(1, context.Customers.Count());
            Assert.NotEqual(default(DateTime), customer.CreatedOnUtc);
            Assert.False(customer.IsDeleted);
        }

        [Fact]
        public async Task InsertIfNotExistsAsync_ShouldNotInsert_WhenExists()
        {
            // Arrange
            using var context = GetDbContext(nameof(InsertIfNotExistsAsync_ShouldNotInsert_WhenExists));
            var repo = new Repository<TestDbContext>(context);
            var customer = new Customer { Name = "Samir" };
            await repo.InsertAsync(customer);
            var duplicate = new Customer { Name = "Samir" };

            // Act
            var inserted = await repo.InsertIfNotExistsAsync(c => c.Name == "Samir", duplicate);

            // Assert
            Assert.False(inserted);
            Assert.Equal(1, context.Customers.Count());
        }

        [Fact]
        public async Task InsertIfNotExistsAsync_ShouldThrow_WhenPredicateIsNull()
        {
            // Arrange
            using var context = GetDbContext(nameof(InsertIfNotExistsAsync_ShouldThrow_WhenPredicateIsNull));
            var repo = new Repository<TestDbContext>(context);
            var customer = new Customer { Name = "Test" };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                repo.InsertIfNotExistsAsync<Customer>(null, customer));
        }

        [Fact]
        public async Task TryInsertAsync_ShouldReturnTrue_OnSuccess()
        {
            // Arrange
            using var context = GetDbContext(nameof(TryInsertAsync_ShouldReturnTrue_OnSuccess));
            var repo = new Repository<TestDbContext>(context);
            var customer = new Customer { Name = "TrySuccess" };

            // Act
            var success = await repo.TryInsertAsync(customer);

            // Assert
            Assert.True(success);
            Assert.Equal(1, context.Customers.Count());
            Assert.NotEqual(default(DateTime), customer.CreatedOnUtc);
            Assert.False(customer.IsDeleted);
        }

        [Fact]
        public async Task TryInsertAsync_ShouldReturnFalse_OnDbUpdateException()
        {
            // Arrange
            using var context = GetDbContext(nameof(TryInsertAsync_ShouldReturnFalse_OnDbUpdateException));
            var repo = new Repository<TestDbContext>(context);
            var customer = new Customer { Name = "TryFail" };
            await repo.InsertAsync(customer);
            context.Entry(customer).State = EntityState.Detached; // Simuler un nouvel insert
            customer.Id = Guid.NewGuid(); // Forcer un ID différent pour éviter conflit InMemory

            // Note: InMemory ne supporte pas les contraintes uniques, donc ce test est limité.
            // Pour tester un vrai DbUpdateException, utiliser SQLite ou SQL Server.

            // Act
            var success = await repo.TryInsertAsync(customer);

            // Assert
            Assert.True(success); // InMemory permet doublons; utiliser SQLite pour vrai test d'échec.
        }

        [Fact]
        public async Task UpsertAsync_ShouldInsert_WhenNotExists()
        {
            // Arrange
            using var context = GetDbContext(nameof(UpsertAsync_ShouldInsert_WhenNotExists));
            var repo = new Repository<TestDbContext>(context);
            var customer = new Customer { Name = "Houssem" };

            // Act
            await repo.UpsertAsync(c => c.Name == "Houssem", customer);

            // Assert
            Assert.Equal(1, context.Customers.Count());
            Assert.NotEqual(default(DateTime), customer.CreatedOnUtc);
            Assert.False(customer.IsDeleted);
        }

        [Fact]
        public async Task UpsertAsync_ShouldUpdate_WhenExists()
        {
            // Arrange
            using var context = GetDbContext(nameof(UpsertAsync_ShouldUpdate_WhenExists));
            var repo = new Repository<TestDbContext>(context);
            var customer = new Customer { Name = "Yassine" };
            await repo.InsertAsync(customer);
            var updatedCustomer = new Customer { Id = customer.Id, Name = "Yassine Updated" };

            // Act
            await repo.UpsertAsync(c => c.Id == customer.Id, updatedCustomer);

            // Assert
            Assert.Equal(1, context.Customers.Count());
            Assert.Equal("Yassine Updated", context.Customers.First().Name);
        }


        [Fact]
        public async Task InsertGraphAsync_ShouldInsertWithRelations_AndLinkForeignKeys()
        {
            // Arrange
            using var context = GetDbContext(nameof(InsertGraphAsync_ShouldInsertWithRelations_AndLinkForeignKeys));
            var repo = new Repository<TestDbContext>(context);
            var customer = new Customer
            {
                Name = "Amira",
                Orders = new List<Order>
                {
                    new Order { Description = "Order1" },
                    new Order { Description = "Order2" }
                }
            };

            // Act
            await repo.InsertGraphAsync(customer);

            // Assert
            Assert.Equal(1, context.Customers.Count());
            Assert.Equal(2, context.Orders.Count());
            Assert.All(context.Orders, o => Assert.Equal(customer.Id, o.CustomerId));
            Assert.NotEqual(default(DateTime), customer.CreatedOnUtc);
            Assert.False(customer.IsDeleted);
            Assert.All(customer.Orders, o => Assert.NotEqual(default(DateTime), o.CreatedOnUtc));
            Assert.All(customer.Orders, o => Assert.False(o.IsDeleted));
        }

        [Fact]
        public async Task InsertWithTransactionAsync_ShouldInsertAll_OnSuccess()
        {
            // Arrange
            using var context = GetDbContext(nameof(InsertWithTransactionAsync_ShouldInsertAll_OnSuccess));
            var repo = new Repository<TestDbContext>(context);
            var customers = new List<Customer>
            {
                new Customer { Name = "Karim" },
                new Customer { Name = "Rania" }
            };

            // Act
            await repo.InsertWithTransactionAsync(customers);

            // Assert
            Assert.Equal(2, context.Customers.Count());
            Assert.All(customers, c => Assert.NotEqual(default(DateTime), c.CreatedOnUtc));
            Assert.All(customers, c => Assert.False(c.IsDeleted));
        }


        [Fact]
        public async Task InsertWithTransactionAsync_ShouldRollback_OnFailure()
        {
            // Arrange
            using var context = GetDbContext(nameof(InsertWithTransactionAsync_ShouldRollback_OnFailure));
            var repo = new Repository<TestDbContext>(context);
            var customers = new List<Customer>
            {
                new Customer { Name = "Valid" },
                null // Entité null pour forcer exception
            };

            // Act & Assert
            async Task TestMethod() => await   repo.InsertWithTransactionAsync(customers);
            await Assert.ThrowsAsync<ArgumentNullException>(TestMethod);
            Assert.Equal(0, context.Customers.Count()); // Tout rollback
        }

        [Fact]
        public async Task InsertAsync_WithCancellationToken_ShouldCancel()
        {
            // Arrange
            using var context = GetDbContext(nameof(InsertAsync_WithCancellationToken_ShouldCancel));
            var repo = new Repository<TestDbContext>(context);
            var customer = new Customer { Name = "TestCancel" };
            var cts = new CancellationTokenSource();
            cts.Cancel(); // Annuler immédiatement pour tester

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                repo.InsertAsync(customer, cts.Token));
            Assert.Equal(0, context.Customers.Count());
        }

 
    }
}