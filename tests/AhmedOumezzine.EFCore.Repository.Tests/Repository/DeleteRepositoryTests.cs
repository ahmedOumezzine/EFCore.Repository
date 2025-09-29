using AhmedOumezzine.EFCore.Repository.Repository;
using AhmedOumezzine.EFCore.Tests.Entity;
using AutoFixture;
using Microsoft.EntityFrameworkCore;

namespace AhmedOumezzine.EFCore.Repository.Tests
{
    [TestClass]
    public class DeleteRepositoryTests : RepositoryTestBase<TestEntity>
    {
        private Fixture _fixture = new();
        private Repository<TestDbContext> _repo;

        [TestInitialize]
        public void TestInitialize()
        {
            RecreateDatabase();
            _repo = CreateRepository();
        }

        #region Soft Delete (Mark as Deleted)

        [TestMethod]
        public async Task DeleteAsync_ShouldSoftDeleteEntityAndSetDeletedProperties()
        {
            // Arrange
            var entity = _fixture.Create<TestEntity>();
            await _repo.InsertAsync(entity);
            var beforeDelete = DateTime.UtcNow;

            // Act
            var rowsAffected = await _repo.DeleteAsync(entity);

            // Assert
            Assert.AreEqual(1, rowsAffected);
            var deletedEntity = CreateDbContext().TestEntities.IgnoreQueryFilters().First();
            Assert.IsTrue(deletedEntity.IsDeleted);
            Assert.IsNotNull(deletedEntity.DeletedOnUtc);
            Assert.IsTrue(deletedEntity.DeletedOnUtc >= beforeDelete);
            Assert.AreEqual(0, CreateDbContext().TestEntities.Count()); // The query filter should hide it.
        }

        [TestMethod]
        public async Task DeleteAsync_MultipleEntities_ShouldSoftDeleteAll()
        {
            // Arrange
            var entities = _fixture.CreateMany<TestEntity>(3).ToList();
            await _repo.InsertRangeAsync(entities);

            // Act
            await _repo.DeleteAsync(entities);

            // Assert
            var count = CreateDbContext().TestEntities.Count();
            Assert.AreEqual(0, count);
            var deletedCount = CreateDbContext().TestEntities.IgnoreQueryFilters().Count();
            Assert.AreEqual(3, deletedCount);
        }

        [TestMethod]
        public async Task DeleteByIdAsync_WhenExists_ShouldSoftDelete()
        {
            // Arrange
            var entity = _fixture.Create<TestEntity>();
            await _repo.InsertAsync(entity);

            // Act
            var result = await _repo.DeleteByIdAsync<TestEntity>(entity.Id);

            // Assert
            Assert.IsTrue(result);
            var deletedEntity = await CreateDbContext().TestEntities.IgnoreQueryFilters().FirstAsync(e => e.Id == entity.Id);
            Assert.IsTrue(deletedEntity.IsDeleted);
        }

        [TestMethod]
        public async Task TryDeleteAsync_WhenSucceeds_ShouldReturnTrue()
        {
            // Arrange
            var entity = _fixture.Create<TestEntity>();
            await _repo.InsertAsync(entity);

            // Act
            var result = await _repo.TryDeleteAsync(entity);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(0, CreateDbContext().TestEntities.Count());
        }

        [TestMethod]
        public async Task DeleteAndReturnAsync_ShouldReturnDeletedEntity()
        {
            // Arrange
            var entity = _fixture.Create<TestEntity>();
            await _repo.InsertAsync(entity);

            // Act
            var result = await _repo.DeleteAndReturnAsync<TestEntity>(entity.Id);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(entity.Id, result.Id);
            Assert.IsTrue(result.IsDeleted);
        }

        #endregion

        //---------------------------------------------------------

        #region Hard Delete (Physical Removal)

        [TestMethod]
        public async Task HardDeleteAsync_ShouldPhysicallyRemoveEntity()
        {
            // Arrange
            var entity = _fixture.Create<TestEntity>();
            await _repo.InsertAsync(entity);

            // Act
            await _repo.HardDeleteAsync(entity);

            // Assert
            var count = CreateDbContext().TestEntities.IgnoreQueryFilters().Count();
            Assert.AreEqual(0, count);
        }

        [TestMethod]
        public async Task HardDeleteAsync_MultipleEntities_ShouldPhysicallyRemoveAll()
        {
            // Arrange
            var entities = _fixture.CreateMany<TestEntity>(3).ToList();
            await _repo.InsertRangeAsync(entities);

            // Act
            await _repo.HardDeleteAsync(entities);

            // Assert
            var count = CreateDbContext().TestEntities.IgnoreQueryFilters().Count();
            Assert.AreEqual(0, count);
        }

        #endregion

        //---------------------------------------------------------

        #region Bulk Delete (EF Core 7+)

        [TestMethod]
        public async Task SoftDeleteFromQueryAsync_ShouldBulkSoftDelete()
        {
            // Arrange
            var entities = new List<TestEntity>
            {
                new TestEntity { Name = "Delete Me" },
                new TestEntity { Name = "Keep Me" },
                new TestEntity { Name = "Delete Me" }
            };
            await _repo.InsertRangeAsync(entities);

            // Act
            var rowsAffected = await _repo.SoftDeleteFromQueryAsync<TestEntity>(e => e.Name == "Delete Me");

            // Assert
            Assert.AreEqual(2, rowsAffected);
            var remaining = CreateDbContext().TestEntities.IgnoreQueryFilters().Where(e => !e.IsDeleted).ToList();
            Assert.AreEqual(1, remaining.Count);
            Assert.AreEqual("Keep Me", remaining.First().Name);
        }

        [TestMethod]
        public async Task DeleteFromQueryAsync_ShouldBulkHardDelete()
        {
            // Arrange
            var entities = new List<TestEntity>
            {
                new TestEntity { Name = "Hard Delete Me" },
                new TestEntity { Name = "Keep Me" },
                new TestEntity { Name = "Hard Delete Me" }
            };
            await _repo.InsertRangeAsync(entities);

            // Act
            var rowsAffected = await _repo.DeleteFromQueryAsync<TestEntity>(e => e.Name == "Hard Delete Me");

            // Assert
            Assert.AreEqual(2, rowsAffected);
            var count = CreateDbContext().TestEntities.IgnoreQueryFilters().Count();
            Assert.AreEqual(1, count);
        }

        [TestMethod]
        public async Task PurgeSoftDeletedAsync_ShouldDeleteOldRecords()
        {
            // Arrange
            var entityToPurge = new TestEntity { Name = "Old", IsDeleted = true, DeletedOnUtc = DateTime.UtcNow.AddDays(-10) };
            var entityToKeep = new TestEntity { Name = "New", IsDeleted = true, DeletedOnUtc = DateTime.UtcNow.AddDays(-1) };

            using var context = CreateDbContext();
            context.TestEntities.AddRange(entityToPurge, entityToKeep);
            await context.SaveChangesAsync();

            // Act
            var rowsAffected = await _repo.PurgeSoftDeletedAsync<TestEntity>(DateTime.UtcNow.AddDays(-5));

            // Assert
            Assert.AreEqual(1, rowsAffected);
            var remaining = context.TestEntities.IgnoreQueryFilters().ToList();
            Assert.AreEqual(1, remaining.Count);
            Assert.AreEqual("New", remaining.First().Name);
        }

        #endregion

        //---------------------------------------------------------

        #region Restore (Undelete)

        [TestMethod]
        public async Task RestoreAsync_ShouldUndeleteEntity()
        {
            // Arrange
            var entity = new TestEntity { IsDeleted = true, DeletedOnUtc = DateTime.UtcNow };
            await _repo.InsertAsync(entity);

            // Act
            var rowsAffected = await _repo.RestoreAsync(entity);

            // Assert
            Assert.AreEqual(1, rowsAffected);
            var restoredEntity = CreateDbContext().TestEntities.First();
            Assert.IsFalse(restoredEntity.IsDeleted);
            Assert.IsNull(restoredEntity.DeletedOnUtc);
        }

        [TestMethod]
        public async Task RestoreRangeAsync_ShouldUndeleteMultipleEntities()
        {
            // Arrange
            var entities = _fixture.CreateMany<TestEntity>(3).ToList();
            foreach (var e in entities) { e.IsDeleted = true; e.DeletedOnUtc = DateTime.UtcNow; }
            await _repo.InsertRangeAsync(entities);

            // Act
            await _repo.RestoreRangeAsync(entities);

            // Assert
            var count = CreateDbContext().TestEntities.Count();
            Assert.AreEqual(3, count);
        }

        [TestMethod]
        public async Task RestoreByIdAsync_ShouldUndeleteSingleEntity()
        {
            // Arrange
            var entity = new TestEntity { IsDeleted = true, DeletedOnUtc = DateTime.UtcNow };
            await _repo.InsertAsync(entity);

            // Act
            var result = await _repo.RestoreByIdAsync<TestEntity>(entity.Id);

            // Assert
            Assert.IsTrue(result);
            var restored = CreateDbContext().TestEntities.First();
            Assert.IsFalse(restored.IsDeleted);
        }

        #endregion
    }
}