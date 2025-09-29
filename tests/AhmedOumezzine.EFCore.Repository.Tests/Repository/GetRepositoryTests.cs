using AhmedOumezzine.EFCore.Repository.Repository;
using AhmedOumezzine.EFCore.Repository.Specification;
using AhmedOumezzine.EFCore.Tests.Entity;
using AutoFixture;
using Microsoft.EntityFrameworkCore;

namespace AhmedOumezzine.EFCore.Repository.Tests
{
    [TestClass]
    public class GetRepositoryTests : RepositoryTestBase<TestEntity>
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
            var child = _fixture.Build<ChildEntity>().With(c => c.ParentId, _parentEntity.Id).Create();
            _parentEntity.Children.Add(child);

            var context = CreateDbContext();
            await context.TestEntities.AddRangeAsync(_activeEntity, _deletedEntity);
            await context.ParentEntities.AddAsync(_parentEntity);
            await context.SaveChangesAsync();
        }

        #region GetAsync - By Condition Tests

        [TestMethod]
        public async Task GetAsync_ByCondition_ShouldReturnEntity()
        {
            // Act
            var entity = await _repo.GetAsync<TestEntity>(e => e.Id == _activeEntity.Id);

            // Assert
            Assert.IsNotNull(entity);
            Assert.AreEqual(_activeEntity.Id, entity.Id);
        }

        [TestMethod]
        public async Task GetAsync_ByCondition_ShouldReturnNullWhenNotFound()
        {
            // Act
            var entity = await _repo.GetAsync<TestEntity>(e => e.Id == Guid.NewGuid());

            // Assert
            Assert.IsNull(entity);
        }

        [TestMethod]
        public async Task GetAsync_WithNullCondition_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repo.GetAsync<TestEntity>(condition:null));
        }

        [TestMethod]
        public async Task GetAsync_WithAsNoTracking_ShouldReturnDetachedEntity()
        {
            // Act
            var entity = await _repo.GetAsync<TestEntity>(e => e.Id == _activeEntity.Id, asNoTracking: true);
            var context = CreateDbContext();

            // Assert
            Assert.IsNotNull(entity);
            Assert.AreEqual(EntityState.Detached, context.Entry(entity).State);
        }

        [TestMethod]
        public async Task GetAsync_WithIncludes_ShouldLoadRelatedData()
        {
            // Act
            var parent = await _repo.GetAsync<ParentEntity>(
                e => e.Id == _parentEntity.Id,
                q => q.Include(p => p.Children));

            // Assert
            Assert.IsNotNull(parent);
            Assert.IsNotNull(parent.Children);
            Assert.AreEqual(1, parent.Children.Count);
        }

        [TestMethod]
        public async Task GetAsync_WithIncludesAndAsNoTracking_ShouldLoadAndDetach()
        {
            // Act
            var parent = await _repo.GetAsync<ParentEntity>(
                e => e.Id == _parentEntity.Id,
                q => q.Include(p => p.Children),
                asNoTracking: true);
            var context = CreateDbContext();

            // Assert
            Assert.IsNotNull(parent);
            Assert.AreEqual(EntityState.Detached, context.Entry(parent).State);
            Assert.AreEqual(1, parent.Children.Count);
        }

        #endregion

        #region GetAsync - By Specification Tests

        [TestMethod]
        public async Task GetAsync_WithSpecification_ShouldReturnEntity()
        {
            // Arrange
            var spec = new TestSpecification(_activeEntity.Id);

            // Act
            var entity = await _repo.GetAsync(spec);

            // Assert
            Assert.IsNotNull(entity);
            Assert.AreEqual(_activeEntity.Id, entity.Id);
        }

        [TestMethod]
        public async Task GetAsync_WithSpecification_ShouldReturnNullWhenNotFound()
        {
            // Arrange
            var spec = new TestSpecification(Guid.NewGuid());

            // Act
            var entity = await _repo.GetAsync(spec);

            // Assert
            Assert.IsNull(entity);
        }

        [TestMethod]
        public async Task GetAsync_WithNullSpecification_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repo.GetAsync<TestEntity>(condition:null));
        }

        [TestMethod]
        public async Task GetAsync_WithSpecificationAndAsNoTracking_ShouldReturnDetachedEntity()
        {
            // Arrange
            var spec = new TestSpecification(_activeEntity.Id);

            // Act
            var entity = await _repo.GetAsync(spec, asNoTracking: true);
            var context = CreateDbContext();

            // Assert
            Assert.IsNotNull(entity);
            Assert.AreEqual(EntityState.Detached, context.Entry(entity).State);
        }

        #endregion

        #region GetAsync - Projection Tests

        [TestMethod]
        public async Task GetAsync_WithProjection_ShouldReturnProjectedObject()
        {
            // Act
            var projected = await _repo.GetAsync<TestEntity, TestProjection>(
                e => e.Id == _activeEntity.Id,
                e => new TestProjection { Name = e.Name });

            // Assert
            Assert.IsNotNull(projected);
            Assert.AreEqual(_activeEntity.Name, projected.Name);
            Assert.IsInstanceOfType(projected, typeof(TestProjection));
        }

        [TestMethod]
        public async Task GetAsync_WithProjection_WithNullCondition_ShouldThrow()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repo.GetAsync<TestEntity, TestProjection>(condition:null, e => new TestProjection()));
        }

        [TestMethod]
        public async Task GetAsync_WithProjection_WithNullSelector_ShouldThrow()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repo.GetAsync<TestEntity, TestProjection>(e => e.Id == _activeEntity.Id, null!));
        }

        [TestMethod]
        public async Task GetAsync_WithSpecificationAndProjection_ShouldReturnProjectedObject()
        {
            // Arrange
            var spec = new TestSpecification(_activeEntity.Id);

            // Act
            var projected = await _repo.GetAsync<TestEntity, TestProjection>(
                spec,
                e => new TestProjection { Name = e.Name });

            // Assert
            Assert.IsNotNull(projected);
            Assert.AreEqual(_activeEntity.Name, projected.Name);
        }

        [TestMethod]
        public async Task GetAsync_WithNullSpecificationAndProjection_ShouldThrow()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repo.GetAsync<TestEntity, TestProjection>(condition:null, e => new TestProjection()));
        }

        [TestMethod]
        public async Task GetAsync_WithSpecificationAndNullSelector_ShouldThrow()
        {
            // Arrange
            var spec = new TestSpecification(_activeEntity.Id);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repo.GetAsync<TestEntity, TestProjection>(spec, null!));
        }

        #endregion

        #region Safe & Utility Methods Tests

        [TestMethod]
        public async Task TryGetAsync_WhenSuccessful_ShouldReturnTrueAndEntity()
        {
            // Act
            var (success, entity) = await _repo.TryGetAsync<TestEntity>(e => e.Id == _activeEntity.Id);

            // Assert
            Assert.IsTrue(success);
            Assert.IsNotNull(entity);
            Assert.AreEqual(_activeEntity.Id, entity.Id);
        }

        [TestMethod]
        public async Task TryGetAsync_WhenNotFound_ShouldReturnFalseAndNull()
        {
            // Act
            var (success, entity) = await _repo.TryGetAsync<TestEntity>(e => e.Id == Guid.NewGuid());

            // Assert
            Assert.IsFalse(success);
            Assert.IsNull(entity);
        }

        [TestMethod]
        public async Task GetFirstOrThrowAsync_WhenFound_ShouldReturnEntity()
        {
            // Act
            var entity = await _repo.GetFirstOrThrowAsync<TestEntity>(e => e.Id == _activeEntity.Id);

            // Assert
            Assert.IsNotNull(entity);
        }

        [TestMethod]
        public async Task GetFirstOrThrowAsync_WhenNotFound_ShouldThrow()
        {
            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _repo.GetFirstOrThrowAsync<TestEntity>(e => e.Id == Guid.NewGuid()));
        }

        [TestMethod]
        public async Task GetFirstOrThrowAsync_WhenNotFound_ShouldThrowWithCustomMessage()
        {
            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _repo.GetFirstOrThrowAsync<TestEntity>(e => e.Id == Guid.NewGuid(), "Entity was not found."));
            Assert.AreEqual("Entity was not found.", ex.Message);
        }

        [TestMethod]
        public async Task ExistsAndFetchAsync_WhenFound_ShouldReturnTrueAndEntity()
        {
            // Act
            var (exists, entity) = await _repo.ExistsAndFetchAsync<TestEntity>(e => e.Id == _activeEntity.Id);

            // Assert
            Assert.IsTrue(exists);
            Assert.IsNotNull(entity);
        }

        [TestMethod]
        public async Task ExistsAndFetchAsync_WhenNotFound_ShouldReturnFalseAndNull()
        {
            // Act
            var (exists, entity) = await _repo.ExistsAndFetchAsync<TestEntity>(e => e.Id == Guid.NewGuid());

            // Assert
            Assert.IsFalse(exists);
            Assert.IsNull(entity);
        }

        [TestMethod]
        public async Task GetOnlyAsync_ShouldReturnSinglePropertyValue()
        {
            // Act
            var name = await _repo.GetOnlyAsync<TestEntity, string>(
                e => e.Id == _activeEntity.Id,
                e => e.Name);

            // Assert
            Assert.AreEqual(_activeEntity.Name, name);
        }

        [TestMethod]
        public async Task GetOnlyAsync_WithNullCondition_ShouldThrow()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _repo.GetOnlyAsync<TestEntity, string>(null!, e => e.Name));
        }

        [TestMethod]
        public async Task GetOnlyAsync_WithNullSelector_ShouldThrow()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _repo.GetOnlyAsync<TestEntity, string>(e => e.Id == _activeEntity.Id, null!));
        }

        [TestMethod]
        public async Task FindAsync_ShouldWorkLikeGetAsync()
        {
            // Act
            var entity = await _repo.FindAsync<TestEntity>(e => e.Id == _activeEntity.Id);

            // Assert
            Assert.IsNotNull(entity);
            Assert.AreEqual(_activeEntity.Id, entity.Id);
        }

        [TestMethod]
        public async Task GetAsyncOrDefault_ShouldWorkLikeGetAsync()
        {
            // Act
            var entity = await _repo.GetAsyncOrDefault<TestEntity>(e => e.Id == _activeEntity.Id);

            // Assert
            Assert.IsNotNull(entity);
            Assert.AreEqual(_activeEntity.Id, entity.Id);
        }

        #endregion

        // Spécification de test pour les méthodes
        private class TestSpecification : Specification<TestEntity>
        {
            public TestSpecification(Guid id)
            {
                Conditions.Add(e => e.Id == id);
            }
        }

    }
}