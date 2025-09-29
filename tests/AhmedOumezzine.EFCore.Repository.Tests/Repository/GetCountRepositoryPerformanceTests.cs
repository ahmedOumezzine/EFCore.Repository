using AhmedOumezzine.EFCore.Repository.Repository;
using AhmedOumezzine.EFCore.Repository.Tests;
using AhmedOumezzine.EFCore.Tests.Entity;
using System.Diagnostics;
using System.Linq.Expressions;

namespace AhmedOumezzine.EFCore.Performance.Tests
{
    [TestClass]
    public class GetCountRepositoryPerformanceTests : RepositoryTestBase<TestEntity>
    {
        private Repository<TestDbContext> _repo;
        private const int LARGE_DATA_COUNT = 500000; // Utiliser un très grand nombre pour un test pertinent
        private string _targetName = "TargetEntity";

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
            entities.Add(new TestEntity { Name = _targetName, Description = "Unique for filtering." });
            await _repo.InsertRangeAsync(entities);
        }

        private void LogPerformance(string methodName, long elapsedMilliseconds)
        {
            Console.WriteLine($"Method: {methodName}");
            Console.WriteLine($"Elapsed Time: {elapsedMilliseconds} ms");
            Console.WriteLine("---------------------------------------------");
        }
        #endregion

        //---------------------------------------------------------

        #region Count Performance Tests

        [TestMethod]
        public async Task Performance_GetCountAsync_WithoutCondition_ShouldBeFast()
        {
            // Act
            var stopwatch = Stopwatch.StartNew();
            var count = await _repo.GetCountAsync<TestEntity>();
            stopwatch.Stop();

            // Assert
            Assert.AreEqual(LARGE_DATA_COUNT + 1, count);
            LogPerformance("GetCountAsync (No Condition)", stopwatch.ElapsedMilliseconds);
        }

        [TestMethod]
        public async Task Performance_GetCountAsync_WithCondition_ShouldBeFast()
        {
            // Act
            var stopwatch = Stopwatch.StartNew();
            var count = await _repo.GetCountAsync<TestEntity>(e => e.Name == _targetName);
            stopwatch.Stop();

            // Assert
            Assert.AreEqual(1, count);
            LogPerformance("GetCountAsync (With Condition)", stopwatch.ElapsedMilliseconds);
        }

        [TestMethod]
        public async Task Performance_GetCountAsync_WithMultipleConditions_ShouldBeFast()
        {
            // Act
            var conditions = new Expression<Func<TestEntity, bool>>[]
            {
                e => e.Name.StartsWith("Entity"),
                e => e.Description == "Performance test data."
            };
            var stopwatch = Stopwatch.StartNew();
            var count = await _repo.GetCountAsync<TestEntity>(conditions);
            stopwatch.Stop();

            // Assert
            Assert.AreEqual(LARGE_DATA_COUNT, count);
            LogPerformance("GetCountAsync (Multiple Conditions)", stopwatch.ElapsedMilliseconds);
        }

        #endregion

        //---------------------------------------------------------

        #region LongCount Performance Tests

        [TestMethod]
        public async Task Performance_GetLongCountAsync_WithoutCondition_ShouldBeFast()
        {
            // Act
            var stopwatch = Stopwatch.StartNew();
            var count = await _repo.GetLongCountAsync<TestEntity>();
            stopwatch.Stop();

            // Assert
            Assert.AreEqual(LARGE_DATA_COUNT + 1, count);
            LogPerformance("GetLongCountAsync (No Condition)", stopwatch.ElapsedMilliseconds);
        }

        #endregion

        //---------------------------------------------------------

        #region HasAnyAsync Performance Tests

        [TestMethod]
        public async Task Performance_HasAnyAsync_WithoutCondition_ShouldBeExtremelyFast()
        {
            // Act
            var stopwatch = Stopwatch.StartNew();
            var result = await _repo.HasAnyAsync<TestEntity>();
            stopwatch.Stop();

            // Assert
            Assert.IsTrue(result);
            LogPerformance("HasAnyAsync (No Condition)", stopwatch.ElapsedMilliseconds);
        }

        [TestMethod]
        public async Task Performance_HasAnyAsync_WithCondition_ShouldBeExtremelyFast()
        {
            // Act
            var stopwatch = Stopwatch.StartNew();
            var result = await _repo.HasAnyAsync<TestEntity>(e => e.Name == _targetName);
            stopwatch.Stop();

            // Assert
            Assert.IsTrue(result);
            LogPerformance("HasAnyAsync (With Condition)", stopwatch.ElapsedMilliseconds);
        }

        #endregion
    }
}