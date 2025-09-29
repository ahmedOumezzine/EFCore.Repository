using AhmedOumezzine.EFCore.Repository.Repository;
using AhmedOumezzine.EFCore.Repository.Specification;
using AhmedOumezzine.EFCore.Tests.Entity;
using AutoFixture;
using Microsoft.EntityFrameworkCore;

namespace AhmedOumezzine.EFCore.Repository.Tests
{
    [TestClass]
    public class GetListRepositoryTests : RepositoryTestBase<TestEntity>
    {
        private Fixture _fixture = new();
        private Repository<TestDbContext> _repo;
        private List<TestEntity> _activeEntities;
        private List<TestEntity> _deletedEntities;

        [TestInitialize]
        public async Task TestInitialize()
        {
            RecreateDatabase();
            _repo = CreateRepository();

            _activeEntities = _fixture.CreateMany<TestEntity>(10).ToList();
            _deletedEntities = _fixture.Build<TestEntity>()
                .With(e => e.IsDeleted, true)
                .CreateMany(5).ToList();

            var context = CreateDbContext();
            await context.TestEntities.AddRangeAsync(_activeEntities);
            await context.TestEntities.AddRangeAsync(_deletedEntities);
            await context.SaveChangesAsync();
        }

        #region Base GetListAsync Overloads

        [TestMethod]
        public async Task GetListAsync_NoParameters_ShouldReturnAllActiveEntities()
        {
            // Act
            var entities = await _repo.GetListAsync<TestEntity>();

            // Assert
            Assert.AreEqual(_activeEntities.Count, entities.Count);
        }

        [TestMethod]
        public async Task GetListAsync_AsNoTracking_ShouldNotTrackEntities()
        {
            // Act
            var entities = await _repo.GetListAsync<TestEntity>(asNoTracking: true);
            var context = CreateDbContext();

            // Assert
            foreach (var entity in entities)
            {
                Assert.AreEqual(EntityState.Detached, context.Entry(entity).State);
            }
        }

        [TestMethod]
        public async Task GetListAsync_WithIncludes_ShouldLoadRelatedData()
        {
            // Arrange
            var parent = _fixture.Create<ParentEntity>();
            var child = _fixture.Build<ChildEntity>().With(c => c.ParentId, parent.Id).Create();
            parent.Children.Add(child);
            await _repo.InsertAsync(parent);

            // Act
            var parents = await _repo.GetListAsync<ParentEntity>(q => q.Include(p => p.Children));

            // Assert
            Assert.IsTrue(parents.Any());
            Assert.AreEqual(1, parents.First().Children.Count);
        }

        [TestMethod]
        public async Task GetListAsync_WithIncludesAndAsNoTracking_ShouldWorkCorrectly()
        {
            // Arrange
            var parent = _fixture.Create<ParentEntity>();
            var child = _fixture.Build<ChildEntity>().With(c => c.ParentId, parent.Id).Create();
            parent.Children.Add(child);
            await _repo.InsertAsync(parent);

            // Act
            var parents = await _repo.GetListAsync<ParentEntity>(
                q => q.Include(p => p.Children),
                asNoTracking: true);

            // Assert
            Assert.AreEqual(EntityState.Detached, CreateDbContext().Entry(parents.First()).State);
            Assert.AreEqual(1, parents.First().Children.Count);
        }

        #endregion

        #region GetListAsync with Condition

        [TestMethod]
        public async Task GetListAsync_WithCondition_ShouldReturnFilteredEntities()
        {
            // Arrange
            var specialEntity = _fixture.Build<TestEntity>()
                .With(e => e.Name, "SpecialName")
                .Create();
            await _repo.InsertAsync(specialEntity);

            // Act
            var entities = await _repo.GetListAsync<TestEntity>(e => e.Name == "SpecialName");

            // Assert
            Assert.AreEqual(1, entities.Count);
            Assert.AreEqual("SpecialName", entities.First().Name);
        }

        #endregion

        #region GetListAsync with Specification

        [TestMethod]
        public async Task GetListAsync_WithSpecification_ShouldFilterAndInclude()
        {
            // Arrange
            var specialEntity = _fixture.Create<TestEntity>();
            var spec = new TestSpecification(specialEntity.Id);

            // Act
            var entities = await _repo.GetListAsync(spec);

            // Assert
            Assert.AreEqual(1, entities.Count);
            Assert.AreEqual(specialEntity.Id, entities.First().Id);
        }

        [TestMethod]
        public async Task GetListAsync_WithSpecificationAndNoTracking_ShouldNotTrackEntities()
        {
            // Arrange
            var spec = new TestSpecification(_activeEntities.First().Id);

            // Act
            var entities = await _repo.GetListAsync(spec, asNoTracking: true);

            // Assert
            Assert.AreEqual(EntityState.Detached, CreateDbContext().Entry(entities.First()).State);
        }

        #endregion

        #region GetListAsync with Projection

        [TestMethod]
        public async Task GetListAsync_WithSelector_ShouldReturnProjectedList()
        {
            // Act
            var projectedList = await _repo.GetListAsync<TestEntity, TestProjection>(e => new TestProjection { Name = e.Name });

            // Assert
            Assert.AreEqual(_activeEntities.Count, projectedList.Count);
            Assert.IsInstanceOfType(projectedList.First(), typeof(TestProjection));
        }

        [TestMethod]
        public async Task GetListAsync_WithConditionAndSelector_ShouldFilterAndProject()
        {
            // Arrange
            var specialEntity = _fixture.Build<TestEntity>().With(e => e.Name, "SpecificName").Create();
            await _repo.InsertAsync(specialEntity);

            // Act
            var projectedList = await _repo.GetListAsync<TestEntity, TestProjection>(
                e => e.Name == "SpecificName",
                e => new TestProjection { Id = e.Id });

            // Assert
            Assert.AreEqual(1, projectedList.Count);
            Assert.AreEqual(specialEntity.Id, projectedList.First().Id);
        }

        [TestMethod]
        public async Task GetListAsync_WithSpecificationAndSelector_ShouldFilterAndProject()
        {
            // Arrange
            var specialEntity = _fixture.Build<TestEntity>().With(e => e.Description, "Description").Create();
            await _repo.InsertAsync(specialEntity);
            var spec = new TestSpecification(description: "Description");

            // Act
            var projectedList = await _repo.GetListAsync<TestEntity, TestProjection>(
                spec,
                e => new TestProjection { Name = e.Name });

            // Assert
            Assert.AreEqual(1, projectedList.Count);
            Assert.AreEqual(specialEntity.Name, projectedList.First().Name);
        }

        [TestMethod]
        public async Task GetListAsync_WithNullSelector_ShouldThrow()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _repo.GetListAsync<TestEntity, TestProjection>(null!));
        }

        #endregion

        #region GetListAsync with Pagination

        [TestMethod]
        public async Task GetListAsync_WithPaginationSpec_ShouldReturnCorrectPage()
        {
            // Arrange
            var spec = new PaginationSpecification<TestEntity>(pageIndex: 1, pageSize: 5);

            // Act
            var paginatedList = await _repo.GetListAsync(spec);

            // Assert
            Assert.AreEqual(5, paginatedList.Items.Count);
            Assert.AreEqual(_activeEntities.Count, paginatedList.TotalItems);
            Assert.AreEqual(3, paginatedList.TotalPages);
        }

        [TestMethod]
        public async Task GetListAsync_WithPaginationAndProjection_ShouldReturnCorrectPage()
        {
            // Arrange
            var spec = new PaginationSpecification<TestEntity>(pageIndex: 1, pageSize: 5);

            // Act
            var paginatedList = await _repo.GetListAsync<TestEntity, TestProjection>(
                spec,
                e => new TestProjection { Id = e.Id, Name = e.Name });

            // Assert
            Assert.AreEqual(5, paginatedList.Items.Count);
            Assert.AreEqual(_activeEntities.Count, paginatedList.TotalItems);
            Assert.IsInstanceOfType(paginatedList.Items.First(), typeof(TestProjection));
        }

   
        [TestMethod]
        public async Task GetListAsync_WithNullPaginationSelector_ShouldThrow()
        {
            // Arrange
            var spec = new PaginationSpecification<TestEntity>(pageIndex: 1, pageSize: 5);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _repo.GetListAsync<TestEntity, TestProjection>(spec, null!));
        }

        #endregion

        #region Specialized & Utility Methods

        [TestMethod]
        public async Task GetDeletedListAsync_ShouldReturnOnlyDeletedEntities()
        {
            // Act
            var deletedList = await _repo.GetDeletedListAsync<TestEntity>();

            // Assert
            Assert.AreEqual(_deletedEntities.Count, deletedList.Count);
            Assert.IsTrue(deletedList.All(e => e.IsDeleted));
        }

        [TestMethod]
        public async Task TryGetListAsync_WhenSuccessful_ShouldReturnSuccessAndItems()
        {
            // Act
            var (success, items) = await _repo.TryGetListAsync<TestEntity>();

            // Assert
            Assert.IsTrue(success);
            Assert.IsNotNull(items);
            Assert.AreEqual(_activeEntities.Count, items.Count);
        }

        [TestMethod]
        public async Task GetDistinctByAsync_ShouldReturnUniqueValues()
        {
            // Arrange
            await _repo.InsertAsync(new TestEntity { Name = "DuplicateName" });
            await _repo.InsertAsync(new TestEntity { Name = "DuplicateName" });

            // Act
            var distinctNames = await _repo.GetDistinctByAsync<TestEntity, string>(e => e.Name);

            // Assert
            Assert.AreEqual(_activeEntities.Count + 1, distinctNames.Count); // 10 initial + "DuplicateName"
            Assert.IsTrue(distinctNames.Contains("DuplicateName"));
        }

        [TestMethod]
        public async Task GetDistinctByAsync_WithNullSelector_ShouldThrow()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _repo.GetDistinctByAsync<TestEntity, string>(null!));
        }

        [TestMethod]
        public async Task ExistsAnyAndListAsync_WithExistingData_ShouldReturnTrueAndList()
        {
            // Act
            var (hasAny, items) = await _repo.ExistsAnyAndListAsync<TestEntity>();

            // Assert
            Assert.IsTrue(hasAny);
            Assert.AreEqual(_activeEntities.Count, items.Count);
        }

        [TestMethod]
        public async Task ExistsAnyAndListAsync_WithNoData_ShouldReturnFalseAndEmptyList()
        {
            // Arrange
            RecreateDatabase();
            _repo = CreateRepository();

            // Act
            var (hasAny, items) = await _repo.ExistsAnyAndListAsync<TestEntity>();

            // Assert
            Assert.IsFalse(hasAny);
            Assert.AreEqual(0, items.Count);
        }

        #endregion
    }

    public class TestSpecification : Specification<TestEntity>
    {
        public TestSpecification(Guid? id = null, string description = null)
        {
            if (id.HasValue)
            {
                Conditions.Add(e => e.Id == id);
            }
            if (description != null)
            {
                Conditions.Add(e => e.Description == description);
            }
        }
    }
}