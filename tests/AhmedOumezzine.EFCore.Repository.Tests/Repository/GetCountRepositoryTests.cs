using AhmedOumezzine.EFCore.Repository.Repository;
using AhmedOumezzine.EFCore.Tests.Entity;
using AutoFixture;
using System.Linq.Expressions;

namespace AhmedOumezzine.EFCore.Repository.Tests
{
    [TestClass]
    public class CountRepositoryTests : RepositoryTestBase<TestEntity>
    {
        private Fixture _fixture = new();
        private Repository<TestDbContext> _repo;
        private const int ACTIVE_COUNT = 10;
        private const int DELETED_COUNT = 5;

        // Déclaration des champs de classe
        private List<TestEntity> _activeEntities;
        private List<TestEntity> _deletedEntities;

        [TestInitialize]
        public async Task TestInitialize()
        {
            RecreateDatabase();
            _repo = CreateRepository();

            // Initialisation des champs
            _activeEntities = _fixture.CreateMany<TestEntity>(ACTIVE_COUNT).ToList();
            _deletedEntities = _fixture.Build<TestEntity>()
                .With(e => e.IsDeleted, true)
                .CreateMany(DELETED_COUNT).ToList();

            var context = CreateDbContext();
            await context.TestEntities.AddRangeAsync(_activeEntities);
            await context.TestEntities.AddRangeAsync(_deletedEntities);
            await context.SaveChangesAsync();
        }

        #region GetCountAsync Tests

        [TestMethod]
        public async Task GetCountAsync_WithoutCondition_ShouldReturnCorrectActiveCount()
        {
            // Act
            var count = await _repo.GetCountAsync<TestEntity>();

            // Assert
            Assert.AreEqual(ACTIVE_COUNT, count);
        }

        [TestMethod]
        public async Task GetCountAsync_WithCondition_ShouldReturnCorrectCount()
        {
            // Arrange
            var entitiesToCount = _fixture.CreateMany<TestEntity>(3).ToList();
            entitiesToCount.ForEach(e => e.Name = "SpecificName");
            await _repo.InsertRangeAsync(entitiesToCount);

            // Act
            var count = await _repo.GetCountAsync<TestEntity>(e => e.Name == "SpecificName");

            // Assert
            Assert.AreEqual(3, count);
        }

        [TestMethod]
        public async Task GetCountAsync_WithConditionOnDeletedEntity_ShouldReturnZero()
        {
            // Arrange
            var deletedEntity = _fixture.Build<TestEntity>()
                .With(e => e.Name, "DeletedName")
                .With(e => e.IsDeleted, true)
                .Create();
            await _repo.InsertAsync(deletedEntity);

            // Act
            var count = await _repo.GetCountAsync<TestEntity>(e => e.Name == "DeletedName");

            // Assert
            Assert.AreEqual(0, count);
        }

        [TestMethod]
        public async Task GetCountAsync_WithNullCondition_ShouldReturnAllActiveCount()
        {
            // Act
            var count = await _repo.GetCountAsync<TestEntity>();

            // Assert
            Assert.AreEqual(ACTIVE_COUNT, count);
        }

        [TestMethod]
        public async Task GetCountAsync_WithMultipleConditions_ShouldReturnCorrectCount()
        {
            // Arrange
            var entity1 = new TestEntity { Name = "CategoryA", Description = "Test" };
            var entity2 = new TestEntity { Name = "CategoryB", Description = "Test" };
            var entity3 = new TestEntity { Name = "CategoryA", Description = "Other" };
            await _repo.InsertRangeAsync(new[] { entity1, entity2, entity3 });

            var conditions = new Expression<Func<TestEntity, bool>>[]
            {
                e => e.Name == "CategoryA",
                e => e.Description == "Test"
            };

            // Act
            var count = await _repo.GetCountAsync<TestEntity>(conditions);

            // Assert
            Assert.AreEqual(1, count);
        }

        [TestMethod]
        public async Task GetCountAsync_WithNullConditions_ShouldReturnAllActiveCount()
        {
            // Act
            var count = await _repo.GetCountAsync<TestEntity>();

            // Assert
            Assert.AreEqual(ACTIVE_COUNT, count);
        }

        #endregion

        #region GetLongCountAsync Tests

        [TestMethod]
        public async Task GetLongCountAsync_WithoutCondition_ShouldReturnCorrectLongCount()
        {
            // Act
            var count = await _repo.GetLongCountAsync<TestEntity>();

            // Assert
            Assert.AreEqual((long)ACTIVE_COUNT, count);
        }

        [TestMethod]
        public async Task GetLongCountAsync_WithCondition_ShouldReturnCorrectLongCount()
        {
            // Arrange
            var entitiesToCount = _fixture.CreateMany<TestEntity>(3).ToList();
            entitiesToCount.ForEach(e => e.Name = "SpecificName");
            await _repo.InsertRangeAsync(entitiesToCount);

            // Act
            var count = await _repo.GetLongCountAsync<TestEntity>(e => e.Name == "SpecificName");

            // Assert
            Assert.AreEqual(3L, count);
        }

        [TestMethod]
        public async Task GetLongCountAsync_WithMultipleConditions_ShouldReturnCorrectLongCount()
        {
            // Arrange
            var entity1 = new TestEntity { Name = "LongCountA", Description = "Test" };
            var entity2 = new TestEntity { Name = "LongCountB", Description = "Test" };
            await _repo.InsertRangeAsync(new[] { entity1, entity2 });

            var conditions = new Expression<Func<TestEntity, bool>>[]
            {
                e => e.Name.StartsWith("LongCount"),
                e => e.Description == "Test"
            };

            // Act
            var count = await _repo.GetLongCountAsync<TestEntity>(conditions);

            // Assert
            Assert.AreEqual(2L, count);
        }

        #endregion

        #region HasAnyAsync Tests

        [TestMethod]
        public async Task HasAnyAsync_WithoutCondition_WhenEntitiesExist_ShouldReturnTrue()
        {
            // Act
            var result = await _repo.HasAnyAsync<TestEntity>();

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task HasAnyAsync_WithCondition_WhenEntityExists_ShouldReturnTrue()
        {
            // Arrange
            var existingEntity = _activeEntities.First();

            // Act
            var result = await _repo.HasAnyAsync<TestEntity>(e => e.Id == existingEntity.Id);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task HasAnyAsync_WithCondition_WhenEntityDoesNotExist_ShouldReturnFalse()
        {
            // Act
            var result = await _repo.HasAnyAsync<TestEntity>(e => e.Id == Guid.NewGuid());

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task HasAnyAsync_WithCondition_WhenOnlyDeletedEntityExists_ShouldReturnFalse()
        {
            // Act
            var result = await _repo.HasAnyAsync<TestEntity>(e => e.Id == _deletedEntities.First().Id);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task HasAnyAsync_WithNullCondition_ShouldReturnTrue()
        {
            // Act
            var result = await _repo.HasAnyAsync<TestEntity>(null!);

            // Assert
            Assert.IsTrue(result);
        }

        #endregion

        #region Soft-Delete Statistics Tests

        [TestMethod]
        public async Task CountSoftDeletedAsync_ShouldReturnCorrectCount()
        {
            // Act
            var count = await _repo.CountSoftDeletedAsync<TestEntity>();

            // Assert
            Assert.AreEqual(DELETED_COUNT, count);
        }

        #endregion

        #region Analytics & Grouping Tests

        [TestMethod]
        public async Task CountByStatusAsync_ShouldReturnCorrectCounts()
        {
            // Arrange
            await _repo.InsertRangeAsync(new[]
            {
                new TestEntity { IsActive = true },
                new TestEntity { IsActive = true },
                new TestEntity { IsActive = false }
            });

            // Act
            var counts = await _repo.CountByStatusAsync<TestEntity>(e => e.IsActive);

            // Assert
            Assert.AreEqual(2, counts[true]);
            Assert.AreEqual(1, counts[false]);
        }

        [TestMethod]
        public async Task CountByDateRangeAsync_ShouldReturnCorrectCount()
        {
            // Arrange
            var start = DateTime.UtcNow.AddDays(-10);
            var end = DateTime.UtcNow.AddDays(-5);

            await _repo.InsertRangeAsync(new[]
            {
                new TestEntity { CreatedOnUtc = DateTime.UtcNow.AddDays(-8) },
                new TestEntity { CreatedOnUtc = DateTime.UtcNow.AddDays(-6) },
                new TestEntity { CreatedOnUtc = DateTime.UtcNow.AddDays(-12) } // Outside range
            });

            // Act
            var count = await _repo.CountByDateRangeAsync<TestEntity>(start, end);

            // Assert
            Assert.AreEqual(2, count);
        }

        #endregion
    }
}