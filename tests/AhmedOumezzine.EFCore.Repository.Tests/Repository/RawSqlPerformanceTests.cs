using AhmedOumezzine.EFCore.Repository.Repository;
using AhmedOumezzine.EFCore.Repository.Tests;
using AhmedOumezzine.EFCore.Tests.Entity;
using AutoFixture;
using System.Diagnostics;

namespace AhmedOumezzine.EFCore.Performance.Tests
{
    [TestClass]
    public class RawSqlPerformanceTests : RepositoryTestBase<TestEntity>
    {
        private Fixture _fixture = new();
        private Repository<TestDbContext> _repo;
        private TestDbContext _dbContext;
        private const int LARGE_DATA_COUNT = 50000;

        [TestInitialize]
        public async Task TestInitialize()
        {
            RecreateDatabase();
            _dbContext = CreateDbContext();
            _repo = CreateRepository();
            await PrepareDataForTest(LARGE_DATA_COUNT);
        }

        #region Helper Methods
        private async Task PrepareDataForTest(int count)
        {
            var entities = _fixture.CreateMany<TestEntity>(count).ToList();
            await _dbContext.TestEntities.AddRangeAsync(entities);
            await _dbContext.SaveChangesAsync();
            _dbContext.ChangeTracker.Clear();
        }

        private void LogPerformance(string methodName, long elapsedMilliseconds)
        {
            Console.WriteLine($"Method: {methodName}");
            Console.WriteLine($"Elapsed Time: {elapsedMilliseconds} ms");
            Console.WriteLine("---------------------------------------------");
        }
        #endregion

        //---------------------------------------------------------

        #region Raw SQL Performance Tests

        [TestMethod]
        public async Task Performance_GetFromRawSqlAsync_ShouldBeFast()
        {
            // Arrange
            var sql = "SELECT * FROM TestEntities";

            // Act
            var stopwatch = Stopwatch.StartNew();
            var entities = await _repo.GetFromRawSqlAsync<TestEntity>(sql);
            stopwatch.Stop();

            // Assert
            Assert.AreEqual(LARGE_DATA_COUNT, entities.Count);
            LogPerformance("GetFromRawSqlAsync (Full Table Scan)", stopwatch.ElapsedMilliseconds);
        }

        [TestMethod]
        public async Task Performance_ExecuteSqlCommandAsync_ShouldBeFast()
        {
            // Arrange
            var sql = "UPDATE TestEntities SET Description = 'Updated' WHERE Description = 'Performance test data.'";

            // Act
            var stopwatch = Stopwatch.StartNew();
            var affectedRows = await _repo.ExecuteSqlCommandAsync(sql);
            stopwatch.Stop();

            // Assert
            Assert.AreEqual(LARGE_DATA_COUNT, affectedRows);
            LogPerformance("ExecuteSqlCommandAsync (Bulk Update)", stopwatch.ElapsedMilliseconds);
        }

        [TestMethod]
        public async Task Performance_ExecuteScalarAsync_ShouldBeFast()
        {
            // Arrange
            var sql = "SELECT COUNT(*) FROM TestEntities";

            // Act
            var stopwatch = Stopwatch.StartNew();
            var count = await _repo.ExecuteScalarAsync<long>(sql);
            stopwatch.Stop();

            // Assert
            Assert.AreEqual(LARGE_DATA_COUNT, count);
            LogPerformance("ExecuteScalarAsync (Count)", stopwatch.ElapsedMilliseconds);
        }

        [TestMethod]
        public async Task Performance_ExecuteInTransactionAsync_ShouldBeFast()
        {
            // Arrange
            var sql = $"UPDATE TestEntities SET Description = 'In Transaction' WHERE Id IN (SELECT Id FROM TestEntities ORDER BY CreatedOnUtc LIMIT 100)";

            // Act
            var stopwatch = Stopwatch.StartNew();
            var affectedRows = await _repo.ExecuteInTransactionAsync(sql);
            stopwatch.Stop();

            // Assert
            Assert.AreEqual(100, affectedRows);
            LogPerformance("ExecuteInTransactionAsync", stopwatch.ElapsedMilliseconds);
        }

        #endregion
    }
}