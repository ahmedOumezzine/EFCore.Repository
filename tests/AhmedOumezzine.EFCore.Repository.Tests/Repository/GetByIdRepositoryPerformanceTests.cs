using AhmedOumezzine.EFCore.Repository.Repository;
using AhmedOumezzine.EFCore.Repository.Tests;
using AhmedOumezzine.EFCore.Tests.Entity;
using System.Diagnostics;

namespace AhmedOumezzine.EFCore.Performance.Tests
{
    [TestClass]
    public class GetByIdRepositoryPerformanceTests : RepositoryTestBase<TestEntity>
    {
        private Repository<TestDbContext> _repo;
        private const int LARGE_DATA_COUNT = 50000;
        private Guid[] _guids;
        private List<TestEntity> _entities;

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
            _entities = new List<TestEntity>();
            for (int i = 0; i < count; i++)
            {
                _entities.Add(new TestEntity
                {
                    Name = $"Entity_{i}",
                    Description = "Performance test data."
                });
            }
            await _repo.InsertRangeAsync(_entities);
            _guids = _entities.Select(e => e.Id).ToArray();
        }

        private void LogPerformance(string methodName, long elapsedMilliseconds, int count)
        {
            double operationsPerSecond = (double)count / (elapsedMilliseconds / 1000.0);
            Console.WriteLine($"Method: {methodName}");
            Console.WriteLine($"Total Operations: {count}");
            Console.WriteLine($"Elapsed Time: {elapsedMilliseconds} ms");
            Console.WriteLine($"Throughput: {operationsPerSecond:F2} ops/sec");
            Console.WriteLine("---------------------------------------------");
        }
        #endregion

        //---------------------------------------------------------

        #region Single-Entity Get Performance Tests

        [TestMethod]
        public async Task Performance_GetByIdAsync_SingleCall_ShouldBeFast()
        {
            // Arrange
            var idToFind = _guids[new Random().Next(0, _guids.Length)];
            var stopwatch = Stopwatch.StartNew();

            // Act
            var entity = await _repo.GetByIdAsync<TestEntity>(idToFind);
            stopwatch.Stop();

            // Assert
            Assert.IsNotNull(entity);
            LogPerformance("GetByIdAsync (single call)", stopwatch.ElapsedMilliseconds, 1);
        }

        [TestMethod]
        public async Task Performance_GetByIdAsync_InLoop_ShouldBeInefficient()
        {
            // Arrange
            var idsToFind = _guids.Take(100).ToList();
            var stopwatch = Stopwatch.StartNew();

            // Act
            foreach (var id in idsToFind)
            {
                await _repo.GetByIdAsync<TestEntity>(id);
            }
            stopwatch.Stop();

            // Assert
            LogPerformance("GetByIdAsync (100 in loop)", stopwatch.ElapsedMilliseconds, idsToFind.Count);
        }

        [TestMethod]
        public async Task Performance_GetProjectedByIdAsync_ShouldBeFast()
        {
            // Arrange
            var idToFind = _guids[new Random().Next(0, _guids.Length)];
            var stopwatch = Stopwatch.StartNew();

            // Act
            var projected = await _repo.GetProjectedByIdAsync<TestEntity, TestProjection>(
                idToFind,
                e => new TestProjection { Name = e.Name });
            stopwatch.Stop();

            // Assert
            Assert.IsNotNull(projected);
            LogPerformance("GetProjectedByIdAsync", stopwatch.ElapsedMilliseconds, 1);
        }

        [TestMethod]
        public async Task Performance_GetPropertyByIdAsync_ShouldBeFast()
        {
            // Arrange
            var idToFind = _guids[new Random().Next(0, _guids.Length)];
            var stopwatch = Stopwatch.StartNew();

            // Act
            var property = await _repo.GetPropertyByIdAsync<TestEntity, string>(
                idToFind,
                e => e.Name);
            stopwatch.Stop();

            // Assert
            Assert.IsNotNull(property);
            LogPerformance("GetPropertyByIdAsync", stopwatch.ElapsedMilliseconds, 1);
        }

        #endregion

        //---------------------------------------------------------

        #region Bulk Load Performance Tests

        [TestMethod]
        public async Task Performance_GetByIdsAsync_ShouldBeHighlyEfficient()
        {
            // Arrange
            var idsToFind = _guids.Take(1000).ToList();
            var stopwatch = Stopwatch.StartNew();

            // Act
            var entities = await _repo.GetByIdsAsync<TestEntity>(idsToFind);
            stopwatch.Stop();

            // Assert
            Assert.AreEqual(idsToFind.Count, entities.Count);
            LogPerformance("GetByIdsAsync (1000 items)", stopwatch.ElapsedMilliseconds, idsToFind.Count);
        }

        #endregion
    }

}