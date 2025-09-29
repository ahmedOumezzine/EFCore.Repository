using AhmedOumezzine.EFCore.Repository.Repository;
using AhmedOumezzine.EFCore.Repository.Specification;
using AhmedOumezzine.EFCore.Repository.Tests;
using AhmedOumezzine.EFCore.Tests.Entity;
using System.Diagnostics;

namespace AhmedOumezzine.EFCore.Performance.Tests
{
    [TestClass]
    public class GetListRepositoryPerformanceTests : RepositoryTestBase<TestEntity>
    {
        private Repository<TestDbContext> _repo;
        private const int LARGE_DATA_COUNT = 50000;
        private string _targetName = "TargetEntity";
        private TestEntity _targetEntity;

        [TestInitialize]
        public async Task TestInitialize()
        {
            RecreateDatabase();
            _repo = CreateRepository();
            await PrepareDataForTest(LARGE_DATA_COUNT);
        }

        #region Helper Methods
        private async Task PrepareDataForTest(int count)
        {
            var entities = new List<TestEntity>();
            for (int i = 0; i < count; i++)
            {
                entities.Add(new TestEntity
                {
                    Name = $"Entity_{i}",
                    Description = "Performance test data."
                });
            }
            _targetEntity = new TestEntity { Name = _targetName, Description = "Unique for filtering." };
            entities.Add(_targetEntity);
            await _repo.InsertRangeAsync(entities);
        }

        private void LogPerformance(string methodName, long elapsedMilliseconds, int count)
        {
            Console.WriteLine($"Method: {methodName}");
            Console.WriteLine($"Total Operations: {count}");
            Console.WriteLine($"Elapsed Time: {elapsedMilliseconds} ms");
            Console.WriteLine("---------------------------------------------");
        }
        #endregion

        //---------------------------------------------------------

        #region Performance Tests

        [TestMethod]
        public async Task Performance_GetListAsync_NoCondition_ShouldBeFast()
        {
            // Act
            var stopwatch = Stopwatch.StartNew();
            var list = await _repo.GetListAsync<TestEntity>();
            stopwatch.Stop();

            // Assert
            Assert.AreEqual(LARGE_DATA_COUNT + 1, list.Count);
            LogPerformance("GetListAsync (No Condition)", stopwatch.ElapsedMilliseconds, list.Count);
        }

        [TestMethod]
        public async Task Performance_GetListAsync_WithCondition_ShouldBeFast()
        {
            // Act
            var stopwatch = Stopwatch.StartNew();
            var list = await _repo.GetListAsync<TestEntity>(e => e.Name == _targetName);
            stopwatch.Stop();

            // Assert
            Assert.AreEqual(1, list.Count);
            LogPerformance("GetListAsync (With Condition)", stopwatch.ElapsedMilliseconds, list.Count);
        }

        [TestMethod]
        public async Task Performance_GetListAsync_WithSpecification_ShouldBeFast()
        {
            // Arrange
            var spec = new TestSpecification(_targetEntity.Id);

            // Act
            var stopwatch = Stopwatch.StartNew();
            var list = await _repo.GetListAsync<TestEntity>(spec);
            stopwatch.Stop();

            // Assert
            Assert.AreEqual(1, list.Count);
            LogPerformance("GetListAsync (With Specification)", stopwatch.ElapsedMilliseconds, list.Count);
        }

        [TestMethod]
        public async Task Performance_GetListAsync_WithProjection_ShouldBeFast()
        {
            // Act
            var stopwatch = Stopwatch.StartNew();
            var projectedList = await _repo.GetListAsync<TestEntity, TestProjection>(e => new TestProjection { Id = e.Id, Name = e.Name });
            stopwatch.Stop();

            // Assert
            Assert.AreEqual(LARGE_DATA_COUNT + 1, projectedList.Count);
            LogPerformance("GetListAsync (With Projection)", stopwatch.ElapsedMilliseconds, projectedList.Count);
        }

        [TestMethod]
        public async Task Performance_GetListAsync_WithPagination_ShouldBeVeryFast()
        {
            // Act
            var spec = new PaginationSpecification<TestEntity>(pageIndex: 1, pageSize: 100);
            var stopwatch = Stopwatch.StartNew();
            var paginatedList = await _repo.GetListAsync(spec);
            stopwatch.Stop();

            // Assert
            Assert.AreEqual(100, paginatedList.Items.Count);
            LogPerformance("GetListAsync (With Pagination)", stopwatch.ElapsedMilliseconds, paginatedList.Items.Count);
        }

        [TestMethod]
        public async Task Performance_GetDeletedListAsync_ShouldBeFast()
        {
            // Arrange
            var deletedEntities = new List<TestEntity>();
            for (int i = 0; i < 1000; i++)
            {
                deletedEntities.Add(new TestEntity { IsDeleted = true });
            }
            await _repo.InsertRangeAsync(deletedEntities);

            // Act
            var stopwatch = Stopwatch.StartNew();
            var list = await _repo.GetDeletedListAsync<TestEntity>();
            stopwatch.Stop();

            // Assert
            Assert.AreEqual(1000, list.Count);
            LogPerformance("GetDeletedListAsync", stopwatch.ElapsedMilliseconds, list.Count);
        }

        #endregion
    }

    public class TestSpecification : Specification<TestEntity>
    {
        public TestSpecification(Guid id)
        {
            Conditions.Add(e => e.Id == id);
        }
    }

    
}