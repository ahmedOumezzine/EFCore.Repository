using AhmedOumezzine.EFCore.Repository.Entities;
using AhmedOumezzine.EFCore.Repository.Entities.AhmedOumezzine.EFCore.Repository.Entities;
using AhmedOumezzine.EFCore.Repository.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

namespace AhmedOumezzine.EFCore.Repository.Tests.UnitTests
{
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
        public async Task InsertAsync_ShouldAddEntity()
        {
            using var context = GetDbContext(nameof(InsertAsync_ShouldAddEntity));
            var repo = new Repository<TestDbContext>(context);

            var customer = new Customer { Name = "Ahmed" };
            var keys = await repo.InsertAsync(customer);

            Assert.NotNull(keys);
            Assert.Equal(1, context.Customers.Count());
        }

        [Fact]
        public async Task InsertRangeAsync_ShouldAddEntities()
        {
            using var context = GetDbContext(nameof(InsertRangeAsync_ShouldAddEntities));
            var repo = new Repository<TestDbContext>(context);

            var customers = new List<Customer>
            {
                new Customer { Name = "Ali" },
                new Customer { Name = "Mouna" }
            };

            await repo.InsertRangeAsync(customers);

            Assert.Equal(2, context.Customers.Count());
        }

        [Fact]
        public async Task InsertWithAuditAsync_ShouldCreateAuditLog()
        {
            using var context = GetDbContext(nameof(InsertWithAuditAsync_ShouldCreateAuditLog));
            var repo = new Repository<TestDbContext>(context);

            var customer = new Customer { Name = "Khaled" };
            await repo.InsertWithAuditAsync(customer, "Admin");

            Assert.Equal(1, context.Customers.Count());
            Assert.Equal(1, context.AuditLogs.Count());
            Assert.Equal("INSERT", context.AuditLogs.First().Action);
        }

        [Fact]
        public async Task InsertIfNotExistsAsync_ShouldInsert_WhenNotExists()
        {
            using var context = GetDbContext(nameof(InsertIfNotExistsAsync_ShouldInsert_WhenNotExists));
            var repo = new Repository<TestDbContext>(context);

            var customer = new Customer { Name = "Fatma" };
            var inserted = await repo.InsertIfNotExistsAsync(c => c.Name == "Fatma", customer);

            Assert.True(inserted);
            Assert.Equal(1, context.Customers.Count());
        }

        [Fact]
        public async Task InsertIfNotExistsAsync_ShouldNotInsert_WhenExists()
        {
            using var context = GetDbContext(nameof(InsertIfNotExistsAsync_ShouldNotInsert_WhenExists));
            var repo = new Repository<TestDbContext>(context);

            var customer = new Customer { Name = "Samir" };
            await repo.InsertAsync(customer);

            var duplicate = new Customer { Name = "Samir" };
            var inserted = await repo.InsertIfNotExistsAsync(c => c.Name == "Samir", duplicate);

            Assert.False(inserted);
            Assert.Equal(1, context.Customers.Count());
        }

        [Fact]
        public async Task InsertIfNotExistsAsync_ShouldReturnFalse_OnDuplicate()
        {
            var dbName = nameof(InsertIfNotExistsAsync_ShouldReturnFalse_OnDuplicate);

            // Premier DbContext : insert initial
            using (var context1 = GetDbContext(dbName))
            {
                var repo1 = new Repository<TestDbContext>(context1);
                var customer = new Customer { Name = "Lina" };
                await repo1.InsertAsync(customer);
            }

            // Nouveau DbContext mais avec le même nom de base → partage la même mémoire
            using (var context2 = GetDbContext(dbName))
            {
                var repo2 = new Repository<TestDbContext>(context2);
                var duplicate = new Customer { Name = "Lina" };

                var inserted = await repo2.InsertIfNotExistsAsync(c => c.Name == "Lina", duplicate);

                // Doit retourner false car doublon détecté
                Assert.False(inserted);
            }

            // Vérifie qu'il n'y a toujours qu'un seul enregistrement
            using (var context3 = GetDbContext(dbName))
            {
                Assert.Equal(1, context3.Customers.Count());
            }
        }

        [Fact]
        public async Task UpsertAsync_ShouldInsert_WhenNotExists()
        {
            using var context = GetDbContext(nameof(UpsertAsync_ShouldInsert_WhenNotExists));
            var repo = new Repository<TestDbContext>(context);

            var customer = new Customer { Name = "Houssem" };
            await repo.UpsertAsync(c => c.Name == "Houssem", customer);

            Assert.Equal(1, context.Customers.Count());
        }

        [Fact]
        public async Task UpsertAsync_ShouldUpdate_WhenExists()
        {
            using var context = GetDbContext(nameof(UpsertAsync_ShouldUpdate_WhenExists));
            var repo = new Repository<TestDbContext>(context);

            var customer = new Customer { Name = "Yassine" };
            await repo.InsertAsync(customer);

            customer.Name = "Yassine Updated";
            await repo.UpsertAsync(c => c.Id == customer.Id, customer);

            Assert.Equal("Yassine Updated", context.Customers.First().Name);
        }

        [Fact]
        public async Task InsertGraphAsync_ShouldInsertWithRelations()
        {
            using var context = GetDbContext(nameof(InsertGraphAsync_ShouldInsertWithRelations));
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

            await repo.InsertGraphAsync(customer);

            Assert.Equal(1, context.Customers.Count());
            Assert.Equal(2, context.Orders.Count());
        }

        [Fact]
        public async Task InsertWithTransactionAsync_ShouldInsertAllOrRollback()
        {
            using var context = GetDbContext(nameof(InsertWithTransactionAsync_ShouldInsertAllOrRollback));
            var repo = new Repository<TestDbContext>(context);

            var customers = new List<Customer>
            {
                new Customer { Name = "Karim" },
                new Customer { Name = "Rania" }
            };

            await repo.InsertWithTransactionAsync(customers);

            Assert.Equal(2, context.Customers.Count());
        }
    }

    #region Test Support Classes

    public class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
    }

    public class Customer : BaseEntity
    {
        public string Name { get; set; }
        public List<Order> Orders { get; set; } = new();
    }

    public class Order : BaseEntity
    {
        public string Description { get; set; }
        public int CustomerId { get; set; }
    }

    public class Product : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;
    }

    #endregion Test Support Classes
}