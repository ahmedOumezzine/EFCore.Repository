using AhmedOumezzine.EFCore.Repository.Repository;
using AhmedOumezzine.EFCore.Tests.Entity;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AhmedOumezzine.EFCore.Repository.Tests
{
    [TestClass]
    public class UpdateRepositoryTests : RepositoryTestBase<TestEntity>
    {
        private Fixture _fixture = new();
        private Repository<TestDbContext> _repo;
        private TestEntity _activeEntity;
        private TestEntity _deletedEntity;

        [TestInitialize]
        public async Task TestInitialize()
        {
            RecreateDatabase();
            _repo = CreateRepository();

            // Initialisation des données
            _activeEntity = _fixture.Create<TestEntity>();
            _deletedEntity = _fixture.Build<TestEntity>()
                .With(e => e.IsDeleted, true)
                .Create();

            var context = CreateDbContext();
            await context.TestEntities.AddRangeAsync(_activeEntity, _deletedEntity);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
        }

        #region Update (Sync) Tests

        [TestMethod]
        public async Task Update_SingleEntity_ShouldMarkAsModified()
        {
            // Arrange
            var entityToUpdate = await _repo.GetByIdAsync<TestEntity>(_activeEntity.Id);
            entityToUpdate.Name = "Updated Name Sync";

            // Act
            _repo.Update(entityToUpdate);

            // Assert
            var context = CreateDbContext();
            var entry = context.Entry(entityToUpdate);
            Assert.AreEqual(EntityState.Modified, entry.State);
        }

        [TestMethod]
        public async Task Update_MultipleEntities_ShouldMarkAllAsModified()
        {
            // Arrange
            var entitiesToUpdate = _repo.GetListAsync<TestEntity>().Result;
            entitiesToUpdate.ForEach(e => e.Name = "Updated Name Batch Sync");

            // Act
            _repo.Update(entitiesToUpdate);

            // Assert
            var context = CreateDbContext();
            foreach (var entity in entitiesToUpdate)
            {
                var entry = context.Entry(entity);
                Assert.AreEqual(EntityState.Modified, entry.State);
            }
        }

        [TestMethod]
        public void Update_WithNullEntity_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _repo.Update<TestEntity>(entity:null));
        }

        [TestMethod]
        public void Update_WithNullEntitiesList_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _repo.Update<TestEntity>(entity: null));
        }

        [TestMethod]
        public void Update_DetachedEntityWithEmptyId_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var detachedEntity = new TestEntity { Id = Guid.Empty };

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _repo.Update(detachedEntity));
        }

        #endregion

        #region Update (Async) Tests

        [TestMethod]
        public async Task UpdateAsync_SingleEntity_ShouldPersistChanges()
        {
            // Arrange
            var entityToUpdate = await _repo.GetByIdAsync<TestEntity>(_activeEntity.Id);
            entityToUpdate.Name = "Updated Name Async";

            // Act
            var affectedRows = await _repo.UpdateAsync(entityToUpdate);

            // Assert
            Assert.AreEqual(1, affectedRows);
            var updatedEntity = await _repo.GetByIdAsync<TestEntity>(_activeEntity.Id);
            Assert.AreEqual("Updated Name Async", updatedEntity.Name);
            Assert.IsTrue(updatedEntity.LastModifiedOnUtc > _activeEntity.LastModifiedOnUtc);
        }

        [TestMethod]
        public async Task UpdateAsync_MultipleEntities_ShouldPersistChanges()
        {
            // Arrange
            var entitiesToUpdate = _repo.GetListAsync<TestEntity>().Result;
            entitiesToUpdate.ForEach(e => e.Name = "Updated Name Batch Async");

            // Act
            var affectedRows = await _repo.UpdateAsync(entitiesToUpdate);

            // Assert
            Assert.AreEqual(entitiesToUpdate.Count, affectedRows);
        }

        #endregion

        #region Update Only (Partial Update) Tests

        [TestMethod]
        public async Task UpdateOnlyAsync_ShouldUpdateOnlySpecifiedProperties()
        {
            // Arrange
            var entityToUpdate = await _repo.GetByIdAsync<TestEntity>(_activeEntity.Id);
            var originalDescription = entityToUpdate.Description;
            entityToUpdate.Name = "Partial Update";
            entityToUpdate.Description = "This should not be updated.";

            // Act
            var affectedRows = await _repo.UpdateOnlyAsync(entityToUpdate, new[] { nameof(TestEntity.Name) });

            // Assert
            Assert.AreEqual(1, affectedRows);
            var updatedEntity = await _repo.GetByIdAsync<TestEntity>(_activeEntity.Id);
            Assert.AreEqual("Partial Update", updatedEntity.Name);
            Assert.AreEqual(originalDescription, updatedEntity.Description);
        }

        [TestMethod]
        public async Task UpdateOnlyAsync_WithNullEntity_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repo.UpdateOnlyAsync<TestEntity>(null!, new[] { "Name" }));
        }

        [TestMethod]
        public async Task UpdateOnlyAsync_WithNullProperties_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repo.UpdateOnlyAsync<TestEntity>(new TestEntity(), null!));
        }

        [TestMethod]
        public async Task UpdateOnlyAsync_DetachedEntityWithEmptyId_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var detachedEntity = new TestEntity { Id = Guid.Empty };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _repo.UpdateOnlyAsync(detachedEntity, new[] { "Name" }));
        }

        #endregion

        #region Conditional Update Tests

        [TestMethod]
        public async Task UpdateIfExistsAsync_WhenEntityExists_ShouldUpdateAndReturnTrue()
        {
            // Arrange
            var entityToUpdate = await _repo.GetByIdAsync<TestEntity>(_activeEntity.Id);
            entityToUpdate.Name = "Updated If Exists";

            // Act
            var result = await _repo.UpdateIfExistsAsync(entityToUpdate);

            // Assert
            Assert.IsTrue(result);
            var updatedEntity = await _repo.GetByIdAsync<TestEntity>(_activeEntity.Id);
            Assert.AreEqual("Updated If Exists", updatedEntity.Name);
        }

        [TestMethod]
        public async Task UpdateIfExistsAsync_WhenEntityDoesNotExist_ShouldReturnFalseAndNotUpdate()
        {
            // Arrange
            var entity = _fixture.Create<TestEntity>();
            entity.Name = "Should Not Exist";

            // Act
            var result = await _repo.UpdateIfExistsAsync(entity);

            // Assert
            Assert.IsFalse(result);
            var notFoundEntity = await _repo.GetByIdAsync<TestEntity>(entity.Id);
            Assert.IsNull(notFoundEntity);
        }

        [TestMethod]
        public async Task UpdateIfExistsAsync_WithNullEntity_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repo.UpdateIfExistsAsync<TestEntity>(null!));
        }

        [TestMethod]
        public async Task UpdateIfExistsAsync_WithEmptyId_ShouldReturnFalse()
        {
            // Arrange
            var entity = new TestEntity { Id = Guid.Empty, Name = "Test" };

            // Act
            var result = await _repo.UpdateIfExistsAsync(entity);

            // Assert
            Assert.IsFalse(result);
        }

        #endregion

        #region Safe Update Tests

        [TestMethod]
        public async Task TryUpdateAsync_WhenSuccessful_ShouldReturnTrue()
        {
            // Arrange
            var entityToUpdate = await _repo.GetByIdAsync<TestEntity>(_activeEntity.Id);
            entityToUpdate.Name = "Try Update";

            // Act
            var result = await _repo.TryUpdateAsync(entityToUpdate);

            // Assert
            Assert.IsTrue(result);
            var updatedEntity = await _repo.GetByIdAsync<TestEntity>(_activeEntity.Id);
            Assert.AreEqual("Try Update", updatedEntity.Name);
        }

        [TestMethod]
        public async Task TryUpdateAsync_WhenEntityDoesNotExist_ShouldReturnFalse()
        {
            // Arrange
            var nonExistentEntity = _fixture.Create<TestEntity>();
            nonExistentEntity.Id = Guid.NewGuid();

            // Act
            var result = await _repo.TryUpdateAsync(nonExistentEntity);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task TryUpdateAsync_WithNullEntity_ShouldReturnFalse()
        {
            // Act
            var result = await _repo.TryUpdateAsync<TestEntity>(null!);

            // Assert
            Assert.IsFalse(result);
        }

        #endregion

        #region Bulk Update (EF Core 7+) Tests

        [TestMethod]
        public async Task UpdateFromQueryAsync_ShouldUpdateEntitiesMatchingPredicate()
        {
            // Arrange
            var entitiesToUpdate = _fixture.CreateMany<TestEntity>(3).ToList();
            entitiesToUpdate.ForEach(e => e.Name = "Bulk Update Target");
            await _repo.InsertRangeAsync(entitiesToUpdate);

            // Act
            var affectedRows = await _repo.UpdateFromQueryAsync<TestEntity>(
                e => e.Name == "Bulk Update Target",
                s => s.SetProperty(e => e.Description, "Updated by Bulk Update"));

            // Assert
            Assert.AreEqual(3, affectedRows);
            var updatedEntities = await _repo.GetListAsync<TestEntity>(e => e.Description == "Updated by Bulk Update");
            Assert.AreEqual(3, updatedEntities.Count);
        }

        [TestMethod]
        public async Task UpdateFromQueryAsync_ShouldAutomaticallySetLastModifiedOnUtc()
        {
            // Arrange
            var entitiesToUpdate = _fixture.CreateMany<TestEntity>(2).ToList();
            await _repo.InsertRangeAsync(entitiesToUpdate);
            var originalLastModified = entitiesToUpdate.First().LastModifiedOnUtc;

            // Act
            await _repo.UpdateFromQueryAsync<TestEntity>(
                e => e.Id == entitiesToUpdate.First().Id,
                s => s.SetProperty(e => e.Name, "New Name"));

            // Assert
            var updatedEntity = await _repo.GetByIdAsync<TestEntity>(entitiesToUpdate.First().Id);
            Assert.IsTrue(updatedEntity.LastModifiedOnUtc > originalLastModified);
        }

        [TestMethod]
        public async Task UpdateFromQueryAsync_WithNullPredicate_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _repo.UpdateFromQueryAsync<TestEntity>(null!, s => s.SetProperty(e => e.Name, "Test")));
        }

        [TestMethod]
        public async Task UpdateFromQueryAsync_WithNullUpdateAction_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _repo.UpdateFromQueryAsync<TestEntity>(e => true, null!));
        }

        [TestMethod]
        public async Task UpdateFromQueryAsync_ShouldNotUpdateSoftDeletedEntities()
        {
            // Arrange
            var softDeletedEntity = _fixture.Build<TestEntity>()
                .With(e => e.IsDeleted, true)
                .With(e => e.Name, "Deleted to update")
                .Create();
            await _repo.InsertAsync(softDeletedEntity);

            // Act
            var affectedRows = await _repo.UpdateFromQueryAsync<TestEntity>(
                e => e.Name == "Deleted to update",
                s => s.SetProperty(e => e.Name, "Updated Deleted"));

            // Assert
            Assert.AreEqual(0, affectedRows);
            var entity = await _repo.GetByIdAsync<TestEntity>(softDeletedEntity.Id);
            Assert.IsNull(entity);
        }

        #endregion
    }
}