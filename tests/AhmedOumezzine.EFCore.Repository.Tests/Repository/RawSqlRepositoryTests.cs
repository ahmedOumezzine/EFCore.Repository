using AhmedOumezzine.EFCore.Repository.Repository;
using AhmedOumezzine.EFCore.Tests.Entity;
using AutoFixture;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace AhmedOumezzine.EFCore.Repository.Tests
{
    [TestClass]
    public class RawSqlRepositoryTests : RepositoryTestBase<TestEntity>
    {
        private Fixture _fixture = new();
        private Repository<TestDbContext> _repo;
        private TestDbContext _dbContext;

        [TestInitialize]
        public async Task TestInitialize()
        {
            RecreateDatabase();
            _dbContext = CreateDbContext();
            _repo = CreateRepository();

            // Préparation des données initiales
            var entities = _fixture.CreateMany<TestEntity>(5).ToList();
            await _dbContext.TestEntities.AddRangeAsync(entities);
            await _dbContext.SaveChangesAsync();
            _dbContext.ChangeTracker.Clear();
        }

        #region ExecuteSqlCommandAsync Tests

        [TestMethod]
        public async Task ExecuteSqlCommandAsync_ShouldExecuteInsertAndReturnAffectedRows()
        {
            // Arrange
            var sql = "INSERT INTO TestEntities (Id, Name, Description, CreatedOnUtc, IsDeleted, IsActive) VALUES (@Id, @Name, @Description, @CreatedOnUtc, @IsDeleted, @IsActive)";
            var newId = Guid.NewGuid();
            var parameters = new object[]
            {
                newId,
                "New Entity",
                "Description",
                DateTime.UtcNow,
                false,
                true
            };

            // Act
            var affectedRows = await _repo.ExecuteSqlCommandAsync(sql, parameters);

            // Assert
            Assert.AreEqual(1, affectedRows);
            var entity = await _dbContext.TestEntities.FindAsync(newId);
            Assert.IsNotNull(entity);
            Assert.AreEqual("New Entity", entity.Name);
        }

        [TestMethod]
        public async Task ExecuteSqlCommandAsync_WithNoParameters_ShouldExecuteUpdate()
        {
            // Arrange
            var entity = _dbContext.TestEntities.First();
            var sql = $"UPDATE TestEntities SET Name = 'Updated Name' WHERE Id = '{entity.Id}'";

            // Act
            var affectedRows = await _repo.ExecuteSqlCommandAsync(sql);

            // Assert
            Assert.AreEqual(1, affectedRows);
            var updatedEntity = await _dbContext.TestEntities.AsNoTracking().FirstOrDefaultAsync(e => e.Id == entity.Id);
            Assert.AreEqual("Updated Name", updatedEntity.Name);
        }

        [TestMethod]
        public async Task ExecuteSqlCommandAsync_WithNullSql_ShouldThrow()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repo.ExecuteSqlCommandAsync(null!));
        }

        [TestMethod]
        public async Task ExecuteSqlCommandAsync_WithEmptySql_ShouldThrow()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repo.ExecuteSqlCommandAsync(string.Empty));
        }

        #endregion

        #region GetFromRawSqlAsync Tests

        [TestMethod]
        public async Task GetFromRawSqlAsync_ShouldReturnListOfEntities()
        {
            // Arrange
            var sql = "SELECT * FROM TestEntities";

            // Act
            var entities = await _repo.GetFromRawSqlAsync<TestEntity>(sql);

            // Assert
            Assert.AreEqual(5, entities.Count);
        }

        [TestMethod]
        public async Task GetFromRawSqlAsync_WithParameters_ShouldFilterEntities()
        {
            // Arrange
            var targetEntity = _dbContext.TestEntities.First();
            var sql = "SELECT * FROM TestEntities WHERE Id = @p0";
            var parameters = new object[] { targetEntity.Id };

            // Act
            var entities = await _repo.GetFromRawSqlAsync<TestEntity>(sql, parameters);

            // Assert
            Assert.AreEqual(1, entities.Count);
            Assert.AreEqual(targetEntity.Id, entities.First().Id);
        }

        [TestMethod]
        public async Task GetFromRawSqlAsync_WithNullSql_ShouldThrow()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repo.GetFromRawSqlAsync<TestEntity>(null!));
        }

        #endregion

        #region ExecuteScalarAsync Tests

        [TestMethod]
        public async Task ExecuteScalarAsync_ShouldReturnSingleValue()
        {
            // Arrange
            var sql = "SELECT COUNT(*) FROM TestEntities";

            // Act
            var count = await _repo.ExecuteScalarAsync<int>(sql);

            // Assert
            Assert.AreEqual(5, count);
        }

        [TestMethod]
        public async Task ExecuteScalarAsync_WithParameters_ShouldReturnCorrectValue()
        {
            // Arrange
            var targetEntity = _dbContext.TestEntities.First();
            var sql = "SELECT COUNT(*) FROM TestEntities WHERE Id = @p0";
            var parameters = new object[] { targetEntity.Id };

            // Act
            var count = await _repo.ExecuteScalarAsync<int>(sql, parameters);

            // Assert
            Assert.AreEqual(1, count);
        }

        [TestMethod]
        public async Task ExecuteScalarAsync_WithNullSql_ShouldThrow()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repo.ExecuteScalarAsync<int>(null!));
        }

        [TestMethod]
        public async Task ExecuteScalarAsync_ShouldHandleNullResult()
        {
            // Arrange
            var sql = "SELECT CAST(NULL AS INT)";

            // Act
            var result = await _repo.ExecuteScalarAsync<int?>(sql);

            // Assert
            Assert.IsNull(result);
        }

        #endregion

        #region GetSingleFromSqlAsync Tests

        [TestMethod]
        public async Task GetSingleFromSqlAsync_ShouldReturnFirstEntity()
        {
            // Arrange
            var sql = "SELECT * FROM TestEntities ORDER BY CreatedOnUtc DESC";
            var expectedEntity = _dbContext.TestEntities.OrderByDescending(e => e.CreatedOnUtc).First();

            // Act
            var entity = await _repo.GetSingleFromSqlAsync<TestEntity>(sql);

            // Assert
            Assert.IsNotNull(entity);
            Assert.AreEqual(expectedEntity.Id, entity.Id);
        }

        [TestMethod]
        public async Task GetSingleFromSqlAsync_WhenNotFound_ShouldReturnNull()
        {
            // Arrange
            var sql = "SELECT * FROM TestEntities WHERE Id = '00000000-0000-0000-0000-000000000000'";

            // Act
            var entity = await _repo.GetSingleFromSqlAsync<TestEntity>(sql);

            // Assert
            Assert.IsNull(entity);
        }

        #endregion

        #region ExistsBySqlAsync Tests

        [TestMethod]
        public async Task ExistsBySqlAsync_WhenExists_ShouldReturnTrue()
        {
            // Arrange
            var sql = "SELECT COUNT(*) FROM TestEntities WHERE Id = @p0";
            var parameters = new object[] { _dbContext.TestEntities.First().Id };

            // Act
            var exists = await _repo.ExistsBySqlAsync(sql, parameters);

            // Assert
            Assert.IsTrue(exists);
        }

        [TestMethod]
        public async Task ExistsBySqlAsync_WhenNotExists_ShouldReturnFalse()
        {
            // Arrange
            var sql = "SELECT COUNT(*) FROM TestEntities WHERE Id = @p0";
            var parameters = new object[] { Guid.NewGuid() };

            // Act
            var exists = await _repo.ExistsBySqlAsync(sql, parameters);

            // Assert
            Assert.IsFalse(exists);
        }

        #endregion

        #region ExecuteInTransactionAsync Tests

        [TestMethod]
        public async Task ExecuteInTransactionAsync_OnSuccess_ShouldCommitChanges()
        {
            // Arrange
            var entity = _dbContext.TestEntities.First();
            var sql = $"UPDATE TestEntities SET Name = 'Transaction Update' WHERE Id = '{entity.Id}'";

            // Act
            var affectedRows = await _repo.ExecuteInTransactionAsync(sql);

            // Assert
            Assert.AreEqual(1, affectedRows);
            var updatedEntity = await _dbContext.TestEntities.AsNoTracking().FirstOrDefaultAsync(e => e.Id == entity.Id);
            Assert.AreEqual("Transaction Update", updatedEntity.Name);
        }

        [TestMethod]
        public async Task ExecuteInTransactionAsync_OnFailure_ShouldRollbackChanges()
        {
            // Arrange
            var entity = _dbContext.TestEntities.First();
            var originalName = entity.Name;
            var sql = $"UPDATE TestEntities SET Name = 'Should Not Commit' WHERE Id = '{entity.Id}'; RAISERROR('Simulated error', 16, 1);";

            // Act & Assert
            await Assert.ThrowsAsync<SqliteException>(() => _repo.ExecuteInTransactionAsync(sql));

            // Re-fetch entity to confirm rollback
            var rolledBackEntity = await _dbContext.TestEntities.AsNoTracking().FirstOrDefaultAsync(e => e.Id == entity.Id);
            Assert.AreEqual(originalName, rolledBackEntity.Name);
        }

        #endregion
    }
}