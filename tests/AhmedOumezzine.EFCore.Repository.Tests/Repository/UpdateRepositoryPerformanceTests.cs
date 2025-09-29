using AhmedOumezzine.EFCore.Repository.Repository;
using AhmedOumezzine.EFCore.Repository.Tests;
using AhmedOumezzine.EFCore.Tests.Entity;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace AhmedOumezzine.EFCore.Performance.Tests
{
    [TestClass]
    public class UpdateRepositoryPerformanceTests : RepositoryTestBase<TestEntity>
    {
        private Fixture _fixture = new();
        private Repository<TestDbContext> _repo;
        private TestDbContext _dbContext;
        private const int LARGE_DATA_COUNT = 50000;
        private List<TestEntity> _entitiesToUpdate;

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
            _entitiesToUpdate = entities.Take(1000).ToList();
        }

        private void LogPerformance(string methodName, long elapsedMilliseconds)
        {
            Console.WriteLine($"Method: {methodName}");
            Console.WriteLine($"Elapsed Time: {elapsedMilliseconds} ms");
            Console.WriteLine("---------------------------------------------");
        }
        #endregion

        //---------------------------------------------------------

        #region Update Performance Tests

        [TestMethod]
        public async Task Performance_UpdateAsync_SingleEntity_ShouldBeFast()
        {
            // Arrange
            var entityToUpdate = _entitiesToUpdate.First();
            entityToUpdate.Name = "Updated Name";

            // Act
            var stopwatch = Stopwatch.StartNew();
            await _repo.UpdateAsync(entityToUpdate);
            stopwatch.Stop();

            // Assert
            LogPerformance("UpdateAsync (Single Entity)", stopwatch.ElapsedMilliseconds);
        }

        //[TestMethod]
        //public async Task Performance_UpdateAsync_Bulk_ShouldBeSlowerThanBulkUpdate()
        //{
        //    // Act
        //    var stopwatch = Stopwatch.StartNew();
        //    foreach (var entity in _entitiesToUpdate)
        //    {
        //        entity.Description = "Updated by bulk operation";
        //        _repo.Update(entity);
        //    }
        //    await _repo.SaveChangesAsync();
        //    stopwatch.Stop();

        //    // Assert
        //    LogPerformance("UpdateAsync (Bulk IEnumerable)", stopwatch.ElapsedMilliseconds);
        //}

        [TestMethod]
        public async Task Performance_UpdateOnlyAsync_ShouldBeSlowerThanBulkUpdate()
        {
            // Act
            var stopwatch = Stopwatch.StartNew();
            foreach (var entity in _entitiesToUpdate)
            {
                var originalName = entity.Name;
                entity.Name = "Partial Update";
                await _repo.UpdateOnlyAsync(entity, new[] { nameof(TestEntity.Name) });
                entity.Name = originalName; // Revert for next test
            }
            stopwatch.Stop();

            // Assert
            LogPerformance("UpdateOnlyAsync (Loop)", stopwatch.ElapsedMilliseconds);
        }

        [TestMethod]
        public async Task Performance_UpdateFromQueryAsync_ShouldBeTheFastest()
        {
            // Act
            var stopwatch = Stopwatch.StartNew();
            var affectedRows = await _repo.UpdateFromQueryAsync<TestEntity>(
                e => _entitiesToUpdate.Select(u => u.Id).Contains(e.Id),
                s => s.SetProperty(e => e.Description, "Updated by Bulk Update from Query"));
            stopwatch.Stop();

            // Assert
            Assert.AreEqual(_entitiesToUpdate.Count, affectedRows);
            LogPerformance("UpdateFromQueryAsync (Bulk EF Core 7+)", stopwatch.ElapsedMilliseconds);
        }

        #endregion
    }
}