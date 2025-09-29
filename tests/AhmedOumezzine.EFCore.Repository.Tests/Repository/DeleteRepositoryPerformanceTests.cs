using AhmedOumezzine.EFCore.Repository.Repository;
using AhmedOumezzine.EFCore.Repository.Tests;
using AhmedOumezzine.EFCore.Tests.Entity;
using System.Diagnostics;

namespace AhmedOumezzine.EFCore.Performance.Tests
{
    [TestClass]
    public class DeleteRepositoryPerformanceTests : RepositoryTestBase<TestEntity>
    {
        private Repository<TestDbContext> _repo;
        private const int LARGE_DATA_COUNT = 5000;
        private const int VERY_LARGE_DATA_COUNT = 50000;

        [TestInitialize]
        public void TestInitialize()
        {
            // Reset the database for each test to ensure consistency
            RecreateDatabase();
            _repo = CreateRepository();
        }

        #region Helper Methods
        private async Task PrepareDataForDeletion(int count, bool isDeleted = false)
        {
            var entities = new List<TestEntity>();
            for (int i = 0; i < count; i++)
            {
                entities.Add(new TestEntity
                {
                    Name = $"TestEntity_{i}",
                    IsDeleted = isDeleted,
                    DeletedOnUtc = isDeleted ? DateTime.UtcNow.AddDays(-1) : (DateTime?)null
                });
            }
            await _repo.InsertRangeAsync(entities);
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

        #region Soft Delete Performance Tests

        [TestMethod]
        public async Task Performance_DeleteAsync_EntityByEntity_ShouldBeSlower()
        {
            // Arrange
            await PrepareDataForDeletion(LARGE_DATA_COUNT);
            var entities = await _repo.GetListAsync<TestEntity>();
            var stopwatch = Stopwatch.StartNew();

            // Act
            foreach (var entity in entities)
            {
                await _repo.DeleteAsync(entity);
            }
            stopwatch.Stop();

            // Assert
            var remainingCount = await _repo.CountAsync<TestEntity>();
            Assert.AreEqual(0, remainingCount);
            LogPerformance("DeleteAsync (Entity-by-Entity)", stopwatch.ElapsedMilliseconds, LARGE_DATA_COUNT);
        }

        [TestMethod]
        public async Task Performance_DeleteAsync_Range_ShouldBeFaster()
        {
            // Arrange
            await PrepareDataForDeletion(LARGE_DATA_COUNT);
            var entities = await _repo.GetListAsync<TestEntity>();
            var stopwatch = Stopwatch.StartNew();

            // Act
            await _repo.DeleteAsync(entities);
            stopwatch.Stop();

            // Assert
            var remainingCount = await _repo.CountAsync<TestEntity>();
            Assert.AreEqual(0, remainingCount);
            LogPerformance("DeleteAsync (Range)", stopwatch.ElapsedMilliseconds, LARGE_DATA_COUNT);
        }

        [TestMethod]
        public async Task Performance_SoftDeleteFromQueryAsync_ShouldBeFastest()
        {
            // Arrange
            await PrepareDataForDeletion(VERY_LARGE_DATA_COUNT);
            var stopwatch = Stopwatch.StartNew();

            // Act
            var rowsAffected = await _repo.SoftDeleteFromQueryAsync<TestEntity>(e => true);
            stopwatch.Stop();

            // Assert
            Assert.AreEqual(VERY_LARGE_DATA_COUNT, rowsAffected);
            LogPerformance("SoftDeleteFromQueryAsync (Bulk)", stopwatch.ElapsedMilliseconds, VERY_LARGE_DATA_COUNT);
        }

        #endregion

        //---------------------------------------------------------

        #region Hard Delete Performance Tests

        [TestMethod]
        public async Task Performance_HardDeleteAsync_Range_ShouldBeFast()
        {
            // Arrange
            await PrepareDataForDeletion(LARGE_DATA_COUNT);
            var entities = await _repo.GetListAsync<TestEntity>();
            var stopwatch = Stopwatch.StartNew();

            // Act
            await _repo.HardDeleteAsync(entities);
            stopwatch.Stop();

            // Assert
            var remainingCount = await _repo.CountAsync<TestEntity>();
            Assert.AreEqual(0, remainingCount);
            LogPerformance("HardDeleteAsync (Range)", stopwatch.ElapsedMilliseconds, LARGE_DATA_COUNT);
        }

        [TestMethod]
        public async Task Performance_DeleteFromQueryAsync_ShouldBeFastest()
        {
            // Arrange
            await PrepareDataForDeletion(VERY_LARGE_DATA_COUNT);
            var stopwatch = Stopwatch.StartNew();

            // Act
            var rowsAffected = await _repo.DeleteFromQueryAsync<TestEntity>(e => true);
            stopwatch.Stop();

            // Assert
            Assert.AreEqual(LARGE_DATA_COUNT, rowsAffected);
            LogPerformance("DeleteFromQueryAsync (Bulk)", stopwatch.ElapsedMilliseconds, VERY_LARGE_DATA_COUNT);
        }

        #endregion

        //---------------------------------------------------------

        #region Restore Performance Tests

        [TestMethod]
        public async Task Performance_RestoreRangeAsync_ShouldBeFast()
        {
            // Arrange
            await PrepareDataForDeletion(LARGE_DATA_COUNT, isDeleted: true);
            var entities = await _repo.GetListAsync<TestEntity>();
            var stopwatch = Stopwatch.StartNew();

            // Act
            await _repo.RestoreRangeAsync(entities);
            stopwatch.Stop();

            // Assert
            var restoredCount = await _repo.CountAsync<TestEntity>();
            Assert.AreEqual(LARGE_DATA_COUNT, restoredCount);
            LogPerformance("RestoreRangeAsync", stopwatch.ElapsedMilliseconds, LARGE_DATA_COUNT);
        }

        #endregion
    }
}