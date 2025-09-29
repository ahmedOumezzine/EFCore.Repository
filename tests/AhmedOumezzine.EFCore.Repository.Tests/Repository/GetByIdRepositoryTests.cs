using AhmedOumezzine.EFCore.Repository.Repository;
using AhmedOumezzine.EFCore.Tests.Entity;
using AutoFixture;
using Microsoft.EntityFrameworkCore;

namespace AhmedOumezzine.EFCore.Repository.Tests
{
    [TestClass]
    public class GetByIdRepositoryTests : RepositoryTestBase<TestEntity>
    {
        private Fixture _fixture = new();
        private Repository<TestDbContext> _repo;
        private TestEntity _activeEntity;
        private TestEntity _deletedEntity;
        private ParentEntity _parentEntity;

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

            _parentEntity = _fixture.Create<ParentEntity>();
            _parentEntity.Children = _fixture.Build<ChildEntity>()
                .With(c => c.ParentId, _parentEntity.Id)
                .CreateMany(2).ToList();

            var context = CreateDbContext();
            await context.TestEntities.AddRangeAsync(_activeEntity, _deletedEntity);
            await context.ParentEntities.AddAsync(_parentEntity);
            await context.SaveChangesAsync();
        }

        #region GetByIdAsync Tests

        [TestMethod]
        public async Task GetByIdAsync_ShouldReturnActiveEntity()
        {
            // Act
            var entity = await _repo.GetByIdAsync<TestEntity>(_activeEntity.Id);

            // Assert
            Assert.IsNotNull(entity);
            Assert.AreEqual(_activeEntity.Id, entity.Id);
            Assert.IsFalse(entity.IsDeleted);
        }

        [TestMethod]
        public async Task GetByIdAsync_ShouldReturnNullForDeletedEntity()
        {
            // Act
            var entity = await _repo.GetByIdAsync<TestEntity>(_deletedEntity.Id);

            // Assert
            Assert.IsNull(entity);
        }

        [TestMethod]
        public async Task GetByIdAsync_ShouldReturnNullWhenNotFound()
        {
            // Act
            var entity = await _repo.GetByIdAsync<TestEntity>(Guid.NewGuid());

            // Assert
            Assert.IsNull(entity);
        }

        [TestMethod]
        public async Task GetByIdAsync_WithIncludes_ShouldLoadRelatedData()
        {
            // Act
            var parent = await _repo.GetByIdAsync<ParentEntity>(
                _parentEntity.Id,
                q => q.Include(p => p.Children));

            // Assert
            Assert.IsNotNull(parent);
            Assert.IsNotNull(parent.Children);
            Assert.AreEqual(2, parent.Children.Count);
        }

        [TestMethod]
        public async Task GetByIdAsync_WithNullIncludes_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repo.GetByIdAsync<TestEntity>(Guid.NewGuid(), null!));
        }

        [TestMethod]
        public async Task GetByIdAsync_WithAsNoTracking_ShouldReturnDetachedEntity()
        {
            // Act
            var entity = await _repo.GetByIdAsync<TestEntity>(_activeEntity.Id, asNoTracking: true);
            var context = CreateDbContext();

            // Assert
            Assert.IsNotNull(entity);
            Assert.AreEqual(EntityState.Detached, context.Entry(entity).State);
        }

        [TestMethod]
        public async Task GetByIdAsync_WithIncludesAndAsNoTracking_ShouldLoadAndDetach()
        {
            // Act
            var parent = await _repo.GetByIdAsync<ParentEntity>(
                _parentEntity.Id,
                q => q.Include(p => p.Children),
                asNoTracking: true);
            var context = CreateDbContext();

            // Assert
            Assert.IsNotNull(parent);
            Assert.AreEqual(EntityState.Detached, context.Entry(parent).State);
            Assert.IsNotNull(parent.Children);
            Assert.AreEqual(2, parent.Children.Count);
        }

        [TestMethod]
        public async Task GetByIdAsync_WithIncludesAndAsNoTracking_WithNullIncludes_ShouldThrow()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repo.GetByIdAsync<TestEntity>(Guid.NewGuid(), null!, true));
        }

        #endregion

        #region Projection Overloads Tests

        [TestMethod]
        public async Task GetProjectedByIdAsync_ShouldReturnProjectedObject()
        {
            // Arrange
            var entityToProject = _fixture.Create<TestEntity>();
            await _repo.InsertAsync(entityToProject);

            // Act
            var projected = await _repo.GetProjectedByIdAsync<TestEntity, TestProjection>(
                entityToProject.Id,
                e => new TestProjection { Name = e.Name });

            // Assert
            Assert.IsNotNull(projected);
            Assert.AreEqual(entityToProject.Name, projected.Name);
        }

        [TestMethod]
        public async Task GetProjectedByIdAsync_WithNullSelector_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _repo.GetProjectedByIdAsync<TestEntity, TestProjection>(Guid.NewGuid(), null!));
        }

        [TestMethod]
        public async Task GetPropertyByIdAsync_ShouldReturnSingleProperty()
        {
            // Act
            var name = await _repo.GetPropertyByIdAsync<TestEntity, string>(
                _activeEntity.Id,
                e => e.Name);

            // Assert
            Assert.AreEqual(_activeEntity.Name, name);
        }

        [TestMethod]
        public async Task GetPropertyByIdAsync_WithNullSelector_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _repo.GetPropertyByIdAsync<TestEntity, string>(Guid.NewGuid(), null!));
        }

        #endregion

        #region Batch Load Tests

        [TestMethod]
        public async Task GetByIdsAsync_ShouldReturnCorrectEntities()
        {
            // Arrange
            var ids = new List<Guid> { _activeEntity.Id, _deletedEntity.Id };

            // Act
            var entities = await _repo.GetByIdsAsync<TestEntity>(ids);

            // Assert
            Assert.AreEqual(1, entities.Count); // Should only return the active one
            Assert.AreEqual(_activeEntity.Id, entities.First().Id);
        }

        [TestMethod]
        public async Task GetByIdsAsync_WithEmptyList_ShouldReturnEmptyList()
        {
            // Act
            var entities = await _repo.GetByIdsAsync<TestEntity>(new List<Guid>());

            // Assert
            Assert.IsNotNull(entities);
            Assert.AreEqual(0, entities.Count);
        }

        [TestMethod]
        public async Task GetByIdsAsync_WithNullList_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repo.GetByIdsAsync<TestEntity>(null!));
        }

        #endregion

        #region Safe Access & Aliases Tests

        [TestMethod]
        public async Task FindByIdAsync_ShouldWorkLikeGetByIdAsync()
        {
            // Act
            var entity = await _repo.FindByIdAsync<TestEntity>(_activeEntity.Id);

            // Assert
            Assert.IsNotNull(entity);
            Assert.AreEqual(_activeEntity.Id, entity.Id);
        }

        [TestMethod]
        public async Task TryGetByIdAsync_WhenSuccessful_ShouldReturnTrueAndEntity()
        {
            // Act
            var (success, entity) = await _repo.TryGetByIdAsync<TestEntity>(_activeEntity.Id);

            // Assert
            Assert.IsTrue(success);
            Assert.IsNotNull(entity);
            Assert.AreEqual(_activeEntity.Id, entity.Id);
        }

        [TestMethod]
        public async Task TryGetByIdAsync_WhenNotFound_ShouldReturnFalseAndNull()
        {
            // Act
            var (success, entity) = await _repo.TryGetByIdAsync<TestEntity>(Guid.NewGuid());

            // Assert
            Assert.IsFalse(success);
            Assert.IsNull(entity);
        }

        #endregion


    }
}