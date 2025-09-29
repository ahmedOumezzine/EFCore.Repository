using AhmedOumezzine.EFCore.Repository.Repository;
using AhmedOumezzine.EFCore.Repository.Tests;
using AhmedOumezzine.EFCore.Tests.Entity;
using AutoFixture;
using System.Diagnostics;

namespace AhmedOumezzine.EFCore.Performance.Tests
{
    [TestClass]
    public class GetPerformanceTests : RepositoryTestBase<TestEntity>
    {
        private Fixture _fixture = new();
        private Repository<TestDbContext> _repo;
        private const int LARGE_DATA_COUNT = 100000;
        private Guid _targetId;
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
            var entities = _fixture.CreateMany<TestEntity>(count).ToList();
            _targetEntity = _fixture.Create<TestEntity>();
            _targetId = _targetEntity.Id;
            entities.Add(_targetEntity);
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

        #region GetAsync Performance Tests

        [TestMethod]
        public async Task Performance_GetAsync_ByCondition_ShouldBeFast()
        {
            // Act
            var stopwatch = Stopwatch.StartNew();
            var entity = await _repo.GetAsync<TestEntity>(e => e.Id == _targetId);
            stopwatch.Stop();

            // Assert
            Assert.IsNotNull(entity);
            Assert.AreEqual(_targetId, entity.Id);
            LogPerformance("GetAsync (By Condition)", stopwatch.ElapsedMilliseconds);
        }

        [TestMethod]
        public async Task Performance_GetAsync_BySpecification_ShouldBeFast()
        {
            // Arrange
            var spec = new TestSpecification(_targetId);

            // Act
            var stopwatch = Stopwatch.StartNew();
            var entity = await _repo.GetAsync(spec);
            stopwatch.Stop();

            // Assert
            Assert.IsNotNull(entity);
            Assert.AreEqual(_targetId, entity.Id);
            LogPerformance("GetAsync (By Specification)", stopwatch.ElapsedMilliseconds);
        }

        [TestMethod]
        public async Task Performance_GetAsync_WithProjection_ShouldBeFast()
        {
            // Act
            var stopwatch = Stopwatch.StartNew();
            var projected = await _repo.GetAsync<TestEntity, TestProjection>(
                e => e.Id == _targetId,
                e => new TestProjection { Name = e.Name });
            stopwatch.Stop();

            // Assert
            Assert.IsNotNull(projected);
            Assert.AreEqual(_targetEntity.Name, projected.Name);
            LogPerformance("GetAsync (With Projection)", stopwatch.ElapsedMilliseconds);
        }

        #endregion
    }


}