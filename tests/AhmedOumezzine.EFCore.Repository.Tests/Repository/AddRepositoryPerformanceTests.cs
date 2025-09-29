using AhmedOumezzine.EFCore.Repository.Repository;
using AhmedOumezzine.EFCore.Tests.Entity;
using global::AhmedOumezzine.EFCore.Repository.Tests;
using System.Diagnostics;

namespace AhmedOumezzine.EFCore.Performance.Tests
{
    [TestClass]
    public class AddRepositoryPerformanceTests : RepositoryTestBase<TestEntity>
    {
        private Repository<TestDbContext> _repo;
        private const int SMALL_BATCH_SIZE = 100;
        private const int LARGE_BATCH_SIZE = 5000;
        private const int VERY_LARGE_BATCH_SIZE = 50000;

        [TestInitialize]
        public void TestInitialize()
        {
            // Always start with a clean database for each test
            RecreateDatabase();
            _repo = CreateRepository();
        }

        #region Performance Measurement Methods

        [TestMethod]
        public async Task Performance_InsertRangeAsync_ShouldBeFastForLargeData()
        {
            // Arrange
            var entities = CreateEntities(LARGE_BATCH_SIZE);
            var stopwatch = Stopwatch.StartNew();

            // Act
            await _repo.InsertRangeAsync(entities);
            stopwatch.Stop();

            // Assert
            var count = CreateDbContext().TestEntities.Count();
            Assert.AreEqual(LARGE_BATCH_SIZE, count);
            LogPerformance("InsertRangeAsync", stopwatch.ElapsedMilliseconds, LARGE_BATCH_SIZE);
        }

        [TestMethod]
        public async Task Performance_InsertManyAsync_WithDifferentBatchSizes_ShouldBeEfficient()
        {
            // Arrange
            var entities = CreateEntities(VERY_LARGE_BATCH_SIZE);
            var batchSize = 1000;
            var stopwatch = Stopwatch.StartNew();

            // Act
            await _repo.InsertManyAsync(entities, batchSize);
            stopwatch.Stop();

            // Assert
            var count = CreateDbContext().TestEntities.Count();
            Assert.AreEqual(VERY_LARGE_BATCH_SIZE, count);
            LogPerformance("InsertManyAsync (batch 1000)", stopwatch.ElapsedMilliseconds, VERY_LARGE_BATCH_SIZE);
        }

        [TestMethod]
        public async Task Performance_InsertWithTransactionAsync_ShouldHaveOverheadButBeFast()
        {
            // Arrange
            var entities = CreateEntities(LARGE_BATCH_SIZE);
            var stopwatch = Stopwatch.StartNew();

            // Act
            await _repo.InsertWithTransactionAsync(entities);
            stopwatch.Stop();

            // Assert
            var count = CreateDbContext().TestEntities.Count();
            Assert.AreEqual(LARGE_BATCH_SIZE, count);
            LogPerformance("InsertWithTransactionAsync", stopwatch.ElapsedMilliseconds, LARGE_BATCH_SIZE);
        }

        [TestMethod]
        public async Task Performance_InsertIfNotExistsAsync_WhenNoEntitiesExist()
        {
            // Arrange
            var entities = CreateEntities(SMALL_BATCH_SIZE);
            var stopwatch = Stopwatch.StartNew();

            // Act
            foreach (var entity in entities)
            {
                await _repo.InsertIfNotExistsAsync(e => e.Name == entity.Name, entity);
            }
            stopwatch.Stop();

            // Assert
            var count = CreateDbContext().TestEntities.Count();
            Assert.AreEqual(SMALL_BATCH_SIZE, count);
            LogPerformance("InsertIfNotExistsAsync (All New)", stopwatch.ElapsedMilliseconds, SMALL_BATCH_SIZE);
        }

        [TestMethod]
        public async Task Performance_InsertWithAuditAsync_ShouldBeSlightlySlower()
        {
            // Arrange
            // We'll insert a single entity multiple times to get a better average
            var totalOperations = 100;
            var stopwatch = Stopwatch.StartNew();

            // Act
            for (int i = 0; i < totalOperations; i++)
            {
                var entity = new TestEntity { Name = $"AuditTest{i}" };
                await _repo.InsertWithAuditAsync(entity, "perfuser");
            }
            stopwatch.Stop();

            // Assert
            var count = CreateDbContext().TestEntities.Count();
            Assert.AreEqual(totalOperations, count);
            LogPerformance("InsertWithAuditAsync", stopwatch.ElapsedMilliseconds, totalOperations);
        }

        #endregion Performance Measurement Methods

        #region Helper Methods

        /// <summary>
        /// Creates a list of entities for bulk operations.
        /// </summary>
        private List<TestEntity> CreateEntities(int count)
        {
            var entities = new List<TestEntity>(count);
            for (int i = 0; i < count; i++)
            {
                entities.Add(new TestEntity
                {
                    Name = $"BulkInsert_{i}",
                    Description = "This is a test entity for performance measurement."
                });
            }
            return entities;
        }

        /// <summary>
        /// Logs performance metrics to the console.
        /// </summary>
        private void LogPerformance(string methodName, long elapsedMilliseconds, int count)
        {
            double entitiesPerSecond = (double)count / (elapsedMilliseconds / 1000.0);
            Console.WriteLine($"Method: {methodName}");
            Console.WriteLine($"Total Entities: {count}");
            Console.WriteLine($"Elapsed Time: {elapsedMilliseconds} ms");
            Console.WriteLine($"Throughput: {entitiesPerSecond:F2} entities/sec");
            Console.WriteLine("---------------------------------------------");
        }

        #endregion Helper Methods
    }
}