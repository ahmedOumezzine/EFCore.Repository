using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq.Expressions;

namespace AhmedOumezzine.EFCore.Repository.Tests.Repository
{
    [TestClass]
    public class ExistsRepositoryTest
    {
        private DbContextOptions<MyDbContext> _options;

        [TestInitialize]
        public void TestInitialize()
        {
            _options = new DbContextOptionsBuilder<MyDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
        }

        [TestMethod]
        public async Task ExistsAsync_EntitiesExist_ShouldReturnTrue()
        {
            // Arrange
            using (var context = new MyDbContext(_options))
            {
                var repository = new AhmedOumezzine.EFCore.Repository.Repository.Repository<MyDbContext>(context);
                var entity = new MyEntity { Name = "demo" };

                context.MyEntities.Add(entity);
                await context.SaveChangesAsync();

                // Act
                bool exists = await repository.ExistsAsync<MyEntity>();

                // Assert
                Assert.IsTrue(exists); // Vérifie qu'au moins une entité de type MyEntity existe
            }
        }

        [TestMethod]
        public async Task ExistsAsync_NoEntitiesExist_ShouldReturnFalse()
        {
            // Arrange
            using (var context = new MyDbContext(_options))
            {
                var repository = new AhmedOumezzine.EFCore.Repository.Repository.Repository<MyDbContext>(context);

                // Act
                bool exists = await repository.ExistsAsync<MyEntity>();

                // Assert
                Assert.IsFalse(exists); // Vérifie qu'aucune entité de type MyEntity n'existe
            }
        }

        [TestMethod]
        public async Task ExistsAsync_WithCondition_EntitiesExist_ShouldReturnTrue()
        {
            // Arrange
            using (var context = new MyDbContext(_options))
            {
                var repository = new AhmedOumezzine.EFCore.Repository.Repository.Repository<MyDbContext>(context);
                var entity = new MyEntity { Name = "demo" };

                context.MyEntities.Add(entity);
                await context.SaveChangesAsync();

                // Act
                Expression<Func<MyEntity, bool>> condition = e => e.Name == "demo";
                bool exists = await repository.ExistsAsync(condition);

                // Assert
                Assert.IsTrue(exists);
            }
        }

        [TestMethod]
        public async Task ExistsAsync_WithCondition_NoEntitiesExist_ShouldReturnFalse()
        {
            // Arrange
            using (var context = new MyDbContext(_options))
            {
                var repository = new AhmedOumezzine.EFCore.Repository.Repository.Repository<MyDbContext>(context);

                // Act
                Expression<Func<MyEntity, bool>> condition = e => e.Name == "nonexistent";
                bool exists = await repository.ExistsAsync(condition);

                // Assert
                Assert.IsFalse(exists);
            }
        }

        [TestMethod]
        public async Task ExistsAsync_NoCondition_EntitiesExist_ShouldReturnTrue()
        {
            // Arrange
            using (var context = new MyDbContext(_options))
            {
                var repository = new AhmedOumezzine.EFCore.Repository.Repository.Repository<MyDbContext>(context);
                var entity = new MyEntity { Name = "demo" };

                context.MyEntities.Add(entity);
                await context.SaveChangesAsync();

                // Act
                bool exists = await repository.ExistsAsync<MyEntity>();

                // Assert
                Assert.IsTrue(exists);
            }
        }

        [TestMethod]
        public async Task ExistsAsync_NoCondition_NoEntitiesExist_ShouldReturnFalse()
        {
            // Arrange
            using (var context = new MyDbContext(_options))
            {
                var repository = new AhmedOumezzine.EFCore.Repository.Repository.Repository<MyDbContext>(context);

                // Act
                bool exists = await repository.ExistsAsync<MyEntity>();

                // Assert
                Assert.IsFalse(exists);
            }
        }
    }
}