using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AhmedOumezzine.EFCore.Repository.Tests.Repository
{
    [TestClass]
    public class DeleteRepositoryTest
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
        public async Task Remove_ValidEntity_ShouldRemoveFromDatabase()
        {
            // Arrange
            using (var context = new MyDbContext(_options))
            {
                var repository = new AhmedOumezzine.EFCore.Repository.Repository.Repository<MyDbContext>(context);
                var entity = new MyEntity { Name = "demo" };

                context.MyEntities.Add(entity);
                await context.SaveChangesAsync();

                // Act
                repository.Remove(entity);
                await context.SaveChangesAsync();

                // Assert
                var retrievedEntity = await context.MyEntities.FindAsync(entity.Id);
                Assert.IsNull(retrievedEntity); // Vérifie que l'entité a été supprimée de la base de données
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Remove_NullEntity_ShouldThrowArgumentNullException()
        {
            // Arrange
            using (var context = new MyDbContext(_options))
            {
                var repository = new AhmedOumezzine.EFCore.Repository.Repository.Repository<MyDbContext>(context);
                MyEntity entity = null;
                // Act
                repository.Remove<MyEntity>(entity);

                // Assert is handled by the ExpectedException attribute
            }
        }

        [TestMethod]
        public async Task Remove_ValidEntities_ShouldRemoveFromDatabase()
        {
            // Arrange
            using (var context = new MyDbContext(_options))
            {
                var repository = new AhmedOumezzine.EFCore.Repository.Repository.Repository<MyDbContext>(context);
                var entities = new List<MyEntity>
                {
                    new MyEntity { Name = "demo1" },
                    new MyEntity { Name = "demo2" },
                };

                context.MyEntities.AddRange(entities);
                await context.SaveChangesAsync();

                // Act
                repository.Remove<MyEntity>(entities);
                await context.SaveChangesAsync();

                // Assert
                foreach (var entity in entities)
                {
                    var retrievedEntity = await context.MyEntities.FindAsync(entity.Id);
                    Assert.IsNull(retrievedEntity); // Vérifie que chaque entité a été supprimée de la base de données
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Remove_NullEntities_ShouldThrowArgumentNullException()
        {
            // Arrange
            using (var context = new MyDbContext(_options))
            {
                var repository = new AhmedOumezzine.EFCore.Repository.Repository.Repository<MyDbContext>(context);
                List<MyEntity> entities = null;
                // Act
                repository.Remove<MyEntity>(entities);

                // Assert is handled by the ExpectedException attribute
            }
        }

        [TestMethod]
        public async Task DeleteAsync_ValidEntity_ShouldRemoveFromDatabase()
        {
            // Arrange
            using (var context = new MyDbContext(_options))
            {
                var repository = new AhmedOumezzine.EFCore.Repository.Repository.Repository<MyDbContext>(context);
                var entity = new MyEntity { Name = "demo" };

                context.MyEntities.Add(entity);
                await context.SaveChangesAsync();

                // Act
                int count = await repository.DeleteAsync(entity);

                // Assert
                var retrievedEntity = await context.MyEntities.FindAsync(entity.Id);
                Assert.IsNull(retrievedEntity); // Vérifie que l'entité a été supprimée de la base de données
                Assert.AreEqual(1, count); // Vérifie que le nombre de changements enregistrés est correct
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DeleteAsync_NullEntity_ShouldThrowArgumentNullException()
        {
            // Arrange
            using (var context = new MyDbContext(_options))
            {
                var repository = new AhmedOumezzine.EFCore.Repository.Repository.Repository<MyDbContext>(context);
                MyEntity entity = null;

                // Act
                await repository.DeleteAsync<MyEntity>(entity);

                // Assert is handled by the ExpectedException attribute
            }
        }

        [TestMethod]
        public async Task DeleteAsync_ValidEntities_ShouldRemoveFromDatabase()
        {
            // Arrange
            using (var context = new MyDbContext(_options))
            {
                var repository = new AhmedOumezzine.EFCore.Repository.Repository.Repository<MyDbContext>(context);
                var entities = new List<MyEntity>
                {
                    new MyEntity { Name = "demo1" },
                    new MyEntity { Name = "demo2" }
                };

                context.MyEntities.AddRange(entities);
                await context.SaveChangesAsync();

                // Act
                int count = await repository.DeleteAsync<MyEntity>(entities);

                // Assert
                foreach (var entity in entities)
                {
                    var retrievedEntity = await context.MyEntities.FindAsync(entity.Id);
                    Assert.IsNull(retrievedEntity); // Vérifie que chaque entité a été supprimée de la base de données
                }
                Assert.AreEqual(2, count); // Vérifie que le nombre de changements enregistrés est correct
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DeleteAsync_NullEntities_ShouldThrowArgumentNullException()
        {
            // Arrange
            using (var context = new MyDbContext(_options))
            {
                var repository = new AhmedOumezzine.EFCore.Repository.Repository.Repository<MyDbContext>(context);
                List<MyEntity> entities = null;

                // Act
                await repository.DeleteAsync<MyEntity>(entities);

                // Assert is handled by the ExpectedException attribute
            }
        }
    }
}