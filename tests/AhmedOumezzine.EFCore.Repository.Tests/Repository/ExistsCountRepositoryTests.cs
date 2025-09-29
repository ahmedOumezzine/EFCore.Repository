using AhmedOumezzine.EFCore.Repository.Repository;
using AhmedOumezzine.EFCore.Tests.Entity;
using AutoFixture;

namespace AhmedOumezzine.EFCore.Repository.Tests
{
    [TestClass]
    public class ExistsCountRepositoryTests : RepositoryTestBase<TestEntity>
    {
        private Fixture _fixture = new();
        private Repository<TestDbContext> _repo;

        [TestInitialize]
        public async Task TestInitialize()
        {
            RecreateDatabase();
            _repo = CreateRepository();
            // Préparer des données de test
            var activeEntity = _fixture.Create<TestEntity>();
            var deletedEntity = _fixture.Build<TestEntity>()
                .With(e => e.IsDeleted, true)
                .Create();
            await _repo.InsertRangeAsync(new[] { activeEntity, deletedEntity });
        }

        #region Exists Tests

        [TestMethod]
        public async Task ExistsAsync_WhenEntitiesExist_ShouldReturnTrue()
        {
            // Arrange
            // Data is prepared in TestInitialize

            // Act
            var result = await _repo.ExistsAsync<TestEntity>();

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task ExistsAsync_WhenOnlyDeletedEntitiesExist_ShouldReturnFalse()
        {
            // Arrange
            RecreateDatabase();
            var repo = CreateRepository();
            var deletedEntity = _fixture.Build<TestEntity>()
                .With(e => e.IsDeleted, true)
                .Create();
            await repo.InsertAsync(deletedEntity);

            // Act
            var result = await repo.ExistsAsync<TestEntity>();

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task ExistsAsync_WithCondition_WhenEntityExists_ShouldReturnTrue()
        {
            // Arrange
            var entityName = "UniqueName";
            var existingEntity = _fixture.Build<TestEntity>()
                .With(e => e.Name, entityName)
                .Create();
            await _repo.InsertAsync(existingEntity);

            // Act
            var result = await _repo.ExistsAsync<TestEntity>(e => e.Name == entityName);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task ExistsAsync_WithCondition_WhenDeletedEntityExists_ShouldReturnFalse()
        {
            // Arrange
            var deletedName = "DeletedName";
            var deletedEntity = _fixture.Build<TestEntity>()
                .With(e => e.Name, deletedName)
                .With(e => e.IsDeleted, true)
                .Create();
            await _repo.InsertAsync(deletedEntity);

            // Act
            var result = await _repo.ExistsAsync<TestEntity>(e => e.Name == deletedName);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task ExistsAsync_WithNullCondition_ShouldThrowArgumentNullException()
        {
            // Arrange
            // No action needed, data is ready

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repo.ExistsAsync<TestEntity>(null!));
        }

        [TestMethod]
        public async Task ExistsByIdAsync_WithValidId_ShouldReturnTrue()
        {
            // Arrange
            var existingEntity = _fixture.Create<TestEntity>();
            await _repo.InsertAsync(existingEntity);

            // Act
            var result = await _repo.ExistsByIdAsync<TestEntity>(existingEntity.Id);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task ExistsByIdAsync_WithNonExistentId_ShouldReturnFalse()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _repo.ExistsByIdAsync<TestEntity>(nonExistentId);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task ExistsByIdAsync_WithDeletedEntityId_ShouldReturnFalse()
        {
            // Arrange
            var deletedEntity = _fixture.Build<TestEntity>()
                .With(e => e.IsDeleted, true)
                .Create();
            await _repo.InsertAsync(deletedEntity);

            // Act
            var result = await _repo.ExistsByIdAsync<TestEntity>(deletedEntity.Id);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task ExistsByIdAsync_WithNullId_ShouldReturnFalse()
        {
            // Act
            var result = await _repo.ExistsByIdAsync<TestEntity>(null!);

            // Assert
            Assert.IsFalse(result);
        }

        #endregion

        //---------------------------------------------------------

        #region Count Tests

        [TestMethod]
        public async Task CountAsync_ShouldReturnCountOfNonDeletedEntities()
        {
            // Arrange
            // TestInitialize ensures 1 active entity and 1 deleted entity

            // Act
            var count = await _repo.CountAsync<TestEntity>();

            // Assert
            Assert.AreEqual(1, count);
        }

        [TestMethod]
        public async Task CountAsync_WithCondition_ShouldReturnCorrectCount()
        {
            // Arrange
            var newEntities = new List<TestEntity>
            {
                new TestEntity { Name = "Category A" },
                new TestEntity { Name = "Category B" },
                new TestEntity { Name = "Category A" }
            };
            await _repo.InsertRangeAsync(newEntities);

            // Act
            var count = await _repo.CountAsync<TestEntity>(e => e.Name == "Category A");

            // Assert
            Assert.AreEqual(2, count);
        }

        [TestMethod]
        public async Task CountAsync_WithCondition_ShouldNotCountDeletedEntities()
        {
            // Arrange
            var newEntities = new List<TestEntity>
            {
                new TestEntity { Name = "Category C" },
                new TestEntity { Name = "Category C", IsDeleted = true }
            };
            await _repo.InsertRangeAsync(newEntities);

            // Act
            var count = await _repo.CountAsync<TestEntity>(e => e.Name == "Category C");

            // Assert
            Assert.AreEqual(1, count);
        }

        [TestMethod]
        public async Task CountAsync_WithNullCondition_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repo.CountAsync<TestEntity>(null!));
        }

        #endregion
    }
}