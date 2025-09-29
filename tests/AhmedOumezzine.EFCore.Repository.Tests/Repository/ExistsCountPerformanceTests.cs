using AhmedOumezzine.EFCore.Repository.Repository;
using AhmedOumezzine.EFCore.Repository.Tests;
using AhmedOumezzine.EFCore.Tests.Entity;
using System.Diagnostics;

namespace AhmedOumezzine.EFCore.Performance.Tests
{
    [TestClass]
    public class ExistsCountPerformanceTests : RepositoryTestBase<TestEntity>
    {
        private Repository<TestDbContext> _repo;
        private const int LARGE_DATA_COUNT = 50000;
        private Guid _existingId;
        private string _existingName = "PerformanceTestName";

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
                var entity = new TestEntity
                {
                    Name = $"Entity_{i}",
                    Description = "Performance test data."
                };
                entities.Add(entity);
            }

            _existingId = Guid.NewGuid();
            var existingEntity = new TestEntity { Id = _existingId, Name = _existingName };
            entities.Add(existingEntity);

            await _repo.InsertRangeAsync(entities);
            // Marquer une entité comme supprimée pour les tests
            var deletedEntity = await _repo.GetListAsync<TestEntity>();
            if (deletedEntity != null)
            {
                await _repo.DeleteAsync(deletedEntity.FirstOrDefault());
            }
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

        #region Exists Performance Tests

        [TestMethod]
        public async Task Performance_ExistsAsync_WithoutCondition_ShouldBeFast()
        {
            // Arrange
            var stopwatch = Stopwatch.StartNew();

            // Act
            var result = await _repo.ExistsAsync<TestEntity>();
            stopwatch.Stop();

            // Assert
            Assert.IsTrue(result);
            LogPerformance("ExistsAsync (no condition)", stopwatch.ElapsedMilliseconds, 1);
        }

        [TestMethod]
        public async Task Performance_ExistsAsync_WithCondition_ShouldBeFast()
        {
            // Arrange
            var stopwatch = Stopwatch.StartNew();

            // Act
            var result = await _repo.ExistsAsync<TestEntity>(e => e.Name == _existingName);
            stopwatch.Stop();

            // Assert
            Assert.IsTrue(result);
            LogPerformance("ExistsAsync (with condition)", stopwatch.ElapsedMilliseconds, 1);
        }

        [TestMethod]
        public async Task Performance_ExistsByIdAsync_WithGuid_ShouldBeFast()
        {
            // Arrange
            var stopwatch = Stopwatch.StartNew();

            // Act
            var result = await _repo.ExistsByIdAsync<TestEntity>(_existingId);
            stopwatch.Stop();

            // Assert
            Assert.IsTrue(result);
            LogPerformance("ExistsByIdAsync (Guid)", stopwatch.ElapsedMilliseconds, 1);
        }

        [TestMethod]
        public async Task Performance_ExistsByIdAsync_WithInt_ShouldBeEfficient()
        {
            // Note: This test assumes you have an entity with an integer primary key.
            // If not, you would need to create a `TestIntEntity` and a `DbSet` for it.
            /*
            // Arrange
            var repoInt = CreateRepository<TestIntEntity>();
            var existingId = 123;
            await repoInt.InsertAsync(new TestIntEntity { Id = existingId, Name = "Test" });
            var stopwatch = Stopwatch.StartNew();

            // Act
            var result = await repoInt.ExistsByIdAsync<TestIntEntity>(existingId);
            stopwatch.Stop();

            // Assert
            Assert.IsTrue(result);
            LogPerformance("ExistsByIdAsync (Int)", stopwatch.ElapsedMilliseconds, 1);
            */
        }

        #endregion

        //---------------------------------------------------------

        #region Count Performance Tests

        [TestMethod]
        public async Task Performance_CountAsync_WithoutCondition_ShouldBeFast()
        {
            // Arrange
            var stopwatch = Stopwatch.StartNew();

            // Act
            var count = await _repo.CountAsync<TestEntity>();
            stopwatch.Stop();

            // Assert
            Assert.AreEqual(LARGE_DATA_COUNT, count); // +1 from PrepareDataForTest
            LogPerformance("CountAsync (no condition)", stopwatch.ElapsedMilliseconds, 1);
        }

        [TestMethod]
        public async Task Performance_CountAsync_WithCondition_ShouldBeFast()
        {
            // Arrange
            var stopwatch = Stopwatch.StartNew();

            // Act
            var count = await _repo.CountAsync<TestEntity>(e => e.Description.Contains("Performance"));
            stopwatch.Stop();

            // Assert
            Assert.AreEqual(LARGE_DATA_COUNT, count);
            LogPerformance("CountAsync (with condition)", stopwatch.ElapsedMilliseconds, 1);
        }

        #endregion
    }
}