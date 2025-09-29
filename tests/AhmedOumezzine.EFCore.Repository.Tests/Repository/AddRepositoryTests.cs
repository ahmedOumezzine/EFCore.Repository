using AhmedOumezzine.EFCore.Repository.Entities;
using AhmedOumezzine.EFCore.Repository.Repository;
using AhmedOumezzine.EFCore.Tests.Entity;
using AhmedOumezzine.EFCore.Tests.Repository;
using AutoFixture;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AhmedOumezzine.EFCore.Repository.Tests
{
    [TestClass]
    public class AddRepositoryTests : RepositoryTestBase<TestEntity>
    {
        private Fixture _fixture = new();

        [TestInitialize]
        public void TestInitialize()
        {
            RecreateDatabase();
        }

        #region InsertAsync

        [TestMethod]
        public async Task InsertAsync_ShouldInsertEntityAndSetAuditProperties()
        {
            // Arrange
            var repo = CreateRepository();
            var entity = _fixture.Build<TestEntity>()
                .Without(e => e.Id) // Laisser Id vide pour qu'il soit généré
                .Without(e => e.CreatedOnUtc)
                .Without(e => e.LastModifiedOnUtc)
                .Create();

            var beforeInsert = DateTime.UtcNow;

            // Act
            var keys = await repo.InsertAsync(entity);
            var insertedEntity = CreateDbContext().TestEntities.First();

            // Assert
            Assert.IsNotNull(keys);
            Assert.AreEqual(1, keys.Length);
            Assert.AreEqual(entity.Id, keys[0]);

            Assert.AreNotEqual(Guid.Empty, insertedEntity.Id);
            Assert.IsTrue(insertedEntity.CreatedOnUtc >= beforeInsert);
            Assert.IsTrue(insertedEntity.LastModifiedOnUtc >= beforeInsert);
            Assert.IsFalse(insertedEntity.IsDeleted);
            Assert.IsNull(insertedEntity.DeletedOnUtc);
            Assert.AreEqual(entity.Name, insertedEntity.Name);
        }

        [TestMethod]
        public async Task InsertAsync_WithProvidedId_ShouldUseIt()
        {
            // Arrange
            var repo = CreateRepository();
            var id = Guid.NewGuid();
            var entity = new TestEntity { Id = id, Name = "Test" };

            // Act
            await repo.InsertAsync(entity);
            var inserted = CreateDbContext().TestEntities.First();

            // Assert
            Assert.AreEqual(id, inserted.Id);
        }

        [TestMethod]
        public async Task InsertAsync_NullEntity_ShouldThrow()
        {
            // Arrange
            var repo = CreateRepository();
            TestEntity? nullEntity = null;

            // Act & Assert

            await Assert.ThrowsExactlyAsync<ArgumentNullException>(() =>
                repo.InsertAsync(nullEntity!));
        }

        #endregion

        #region InsertRangeAsync

        [TestMethod]
        public async Task InsertRangeAsync_ShouldInsertMultipleEntities()
        {
            // Arrange
            var repo = CreateRepository();
            var entities = _fixture.CreateMany<TestEntity>(3).ToList();

            // Act
            await repo.InsertRangeAsync(entities);
            var count = CreateDbContext().TestEntities.Count();

            // Assert
            Assert.AreEqual(3, count);
            foreach (var entity in entities)
            {
                Assert.AreNotEqual(Guid.Empty, entity.Id);
                Assert.IsFalse(entity.IsDeleted);
            }
        }

        [TestMethod]
        public async Task InsertRangeAsync_NullCollection_ShouldThrow()
        {
            // Arrange
            var repo = CreateRepository();
            IEnumerable<TestEntity>? nullEntities = null;

            // Act & Assert
            await Assert.ThrowsExactlyAsync<ArgumentNullException>(() =>
                repo.InsertRangeAsync(nullEntities!));
        }

        #endregion

        #region InsertWithAuditAsync

        [TestMethod]
        public async Task InsertWithAuditAsync_ShouldInsertAndNotCrash_WhenAuditLogNotInModel()
        {
            // Arrange
            var repo = CreateRepository();
            var entity = new TestEntity { Name = "AuditTest" };

            // Act
            var keys = await repo.InsertWithAuditAsync(entity, "testuser");

            // Assert
            Assert.IsNotNull(keys);
            var inserted = CreateDbContext().TestEntities.First();
            Assert.AreEqual("AuditTest", inserted.Name);
            // Pas d'erreur même si AuditLog n'est pas dans le modèle
        }
        [TestMethod]
        public async Task InsertWithAuditAsync_ShouldCreateAuditLog()
        {
            // Arrange
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            SQLitePCL.Batteries_V2.Init();

            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseSqlite(connection)
                .Options;

            using var context = new TestDbContext(options);
            context.Database.EnsureCreated();

            var repo = new Repository<TestDbContext>(context);
            var entity = new TestEntity { Name = "AuditTest" };

            // Act
            await repo.InsertWithAuditAsync(entity, "testuser");

            // Assert
            Assert.AreEqual(1, context.TestEntities.Count());
            Assert.AreEqual(1, context.AuditLogs.Count());

            var audit = context.AuditLogs.First();
            Assert.AreEqual("INSERT", audit.Action);
            Assert.AreEqual("TestEntity", audit.EntityName);
            Assert.AreEqual(entity.Id.ToString(), audit.EntityId);
            Assert.AreEqual("testuser", audit.UserName);
        }

        #endregion

        #region InsertIfNotExistsAsync

        [TestMethod]
        public async Task InsertIfNotExistsAsync_WhenNotExists_ShouldInsert()
        {
            // Arrange
            var repo = CreateRepository();
            var entity = new TestEntity { Name = "Unique" };

            // Act
            var result = await repo.InsertIfNotExistsAsync(e => e.Name == "Unique", entity);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(1, CreateDbContext().TestEntities.Count());
        }

        [TestMethod]
        public async Task InsertIfNotExistsAsync_WhenExists_ShouldNotInsert()
        {
            // Arrange
            var context = CreateDbContext();
            var existing = new TestEntity { Name = "Existing" };
            context.TestEntities.Add(existing);
            await context.SaveChangesAsync();

            var repo = CreateRepository();
            var newEntity = new TestEntity { Name = "Existing", Description = "New" };

            // Act
            var result = await repo.InsertIfNotExistsAsync(e => e.Name == "Existing", newEntity);

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(1, CreateDbContext().TestEntities.Count());
        }

        [TestMethod]
        public async Task InsertIfNotExistsAsync_NullPredicate_ShouldThrow()
        {
            var repo = CreateRepository();
            var entity = new TestEntity { Name = "Test" };
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                (Task)repo.InsertIfNotExistsAsync<TestEntity>(null!, entity));
        }

        #endregion

        #region TryInsertAsync

        [TestMethod]
        public async Task TryInsertAsync_ValidEntity_ShouldReturnTrue()
        {
            // Arrange
            var repo = CreateRepository();
            var entity = new TestEntity { Name = "TryInsert" };

            // Act
            var result = await repo.TryInsertAsync(entity);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task TryInsertAsync_WhenFails_ShouldReturnFalse()
        {
            var repo = CreateRepository();

            // Entité invalide (ex: Name trop long)
            var invalidEntity = new TestEntity { Name = new string('x', 200) };

            var result = await repo.TryInsertAsync(invalidEntity);
            Assert.IsFalse(result);

            // Vérifier qu'aucune entité n'a été insérée
            using var context = CreateDbContext();
            Assert.AreEqual(0, context.TestEntities.Count());
        }

        #endregion

        #region UpsertAsync

        [TestMethod]
        public async Task UpsertAsync_WhenNotExists_ShouldInsert()
        {
            // Arrange
            var repo = CreateRepository();
            var entity = new TestEntity { Name = "UpsertNew" };

            // Act
            await repo.UpsertAsync(e => e.Name == "UpsertNew", entity);

            // Assert
            var count = CreateDbContext().TestEntities.Count();
            Assert.AreEqual(1, count);
        }

        [TestMethod]
        public async Task UpsertAsync_WhenExists_ShouldUpdate()
        {
            // Arrange
            var context = CreateDbContext();
            var existing = new TestEntity { Name = "UpsertExists", Description = "Old" };
            context.TestEntities.Add(existing);
            await context.SaveChangesAsync();

            var repo = CreateRepository();
            var updated = new TestEntity
            {
                Id = existing.Id,
                Name = "UpsertExists",
                Description = "New"
            };

            // Act
            await repo.UpsertAsync(e => e.Name == "UpsertExists", updated);

            // Assert
            var result = CreateDbContext().TestEntities.First();
            Assert.AreEqual("New", result.Description);
            Assert.IsTrue(result.LastModifiedOnUtc > result.CreatedOnUtc);
        }

        [TestMethod]
        public async Task UpsertAsync_NullPredicate_ShouldThrow()
        {
            var repo = CreateRepository();
            var entity = new TestEntity { Name = "Test" };
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                (Task)repo.UpsertAsync<TestEntity>(null!, entity));
        }

        [TestMethod]
        public async Task UpsertAsync_NullEntity_ShouldThrow()
        {
            var repo = CreateRepository();
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                (Task)repo.UpsertAsync<TestEntity>(e => e.Name == "Test", null!));
        }
        #endregion

        #region InsertWithTransactionAsync

        [TestMethod]
        public async Task InsertWithTransactionAsync_ShouldInsertAllOrNothing()
        {
            // Arrange
            var repo = CreateRepository();
            var entities = new[]
            {
                new TestEntity { Name = "Tx1" },
                new TestEntity { Name = "Tx2" }
            };

            // Act
            await repo.InsertWithTransactionAsync(entities);

            // Assert
            var count = CreateDbContext().TestEntities.Count();
            Assert.AreEqual(2, count);
        }

        [TestMethod]
        public async Task InsertWithTransactionAsync_NullEntities_ShouldThrow()
        {
            var repo = CreateRepository();
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                 repo.InsertWithTransactionAsync<TestEntity>(null));
        }

        [TestMethod]
        public async Task InsertWithTransactionAsync_WhenFails_ShouldRollback()
        {
            var repo = CreateRepository();

            // Créer une entité invalide (ex: violation de longueur)
            var invalidEntity = new TestEntity
            {
                Name = new string('x', 200) // Supposé > 100 caractères → échoue
            };

            try
            {
                await repo.InsertWithTransactionAsync(new[] { invalidEntity });
                Assert.Fail("Expected DbUpdateException");
            }
            catch (Exception)
            {
                // Vérifier que rien n'a été inséré
                using var context = CreateDbContext();
                Assert.AreEqual(0, context.TestEntities.Count());
            }
        }
        #endregion

        [TestMethod]
        public async Task InsertManyAsync_ShouldInsertInBatches()
        {
            var repo = CreateRepository();
            var entities = Enumerable.Range(1, 1200)
                .Select(i => new TestEntity { Name = $"Item{i}" })
                .ToList();

            await repo.InsertManyAsync(entities, batchSize: 500);

            using var context = CreateDbContext();
            Assert.AreEqual(1200, context.TestEntities.Count());
        }

        [TestMethod]
        public async Task InsertAndReturnAsync_ShouldReturnEntityWithId()
        {
            var repo = CreateRepository();
            var entity = new TestEntity { Name = "Returned" };

            var result = await repo.InsertAndReturnAsync(entity);

            Assert.IsNotNull(result);
            Assert.AreEqual(entity.Id, result.Id);
            Assert.AreNotEqual(Guid.Empty, result.Id);
        }
    }
}