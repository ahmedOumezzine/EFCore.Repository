using AhmedOumezzine.EFCore.Repository.Tests.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AhmedOumezzine.EFCore.Repository.Tests
{
    [TestClass]
    public class GetByIdRepositoryTest
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
        public async Task GetByIdAsync_EntityExists_ShouldReturnEntity()
        {
            // Arrange
            using (var context = new MyDbContext(_options))
            {
                var repository = new AhmedOumezzine.EFCore.Repository.Repository.Repository<MyDbContext>(context);
                var entity = new MyEntity { Name = "demo" };

                context.MyEntities.Add(entity);
                await context.SaveChangesAsync();

                // Act
                var retrievedEntity = await repository.GetByIdAsync<MyEntity>(entity.Id);

                // Assert
                Assert.IsNotNull(retrievedEntity);
                Assert.AreEqual(entity.Id, retrievedEntity.Id);
            }
        }

        [TestMethod]
        public async Task GetByIdAsync_EntityDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            using (var context = new MyDbContext(_options))
            {
                var repository = new AhmedOumezzine.EFCore.Repository.Repository.Repository<MyDbContext>(context);

                // Act
                var retrievedEntity = await repository.GetByIdAsync<MyEntity>(Guid.NewGuid());

                // Assert
                Assert.IsNull(retrievedEntity);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetByIdAsync_NullId_ShouldThrowArgumentNullException()
        {
            // Arrange
            using (var context = new MyDbContext(_options))
            {
                var repository = new AhmedOumezzine.EFCore.Repository.Repository.Repository<MyDbContext>(context);

                // Act
                await repository.GetByIdAsync<MyEntity>(null);

                // Assert is handled by the ExpectedException attribute
            }
        }

        [TestMethod]
        public async Task GetByIdAsync_WithIncludes_ShouldReturnEntityWithIncludedData()
        {
            // Arrange
            using (var context = new MyDbContext(_options))
            {
                var repository = new AhmedOumezzine.EFCore.Repository.Repository.Repository<MyDbContext>(context);
                var entity = new MyEntity { Name = "demo", RelatedEntity = new MyRelatedEntity { RelatedData = "related" } };

                context.MyEntities.Add(entity);
                await context.SaveChangesAsync();

                // Act
                var retrievedEntity = await repository.GetByIdAsync<MyEntity>(entity.Id, q => q.Include(e => e.RelatedEntity), false);

                // Assert
                Assert.IsNotNull(retrievedEntity);
                Assert.IsNotNull(retrievedEntity.RelatedEntity);
                Assert.AreEqual("related", retrievedEntity.RelatedEntity.RelatedData);
            }
        }

        [TestMethod]
        public async Task GetByIdAsync_AsNoTracking_ShouldReturnEntityWithoutTracking()
        {
            // Arrange
            using (var context = new MyDbContext(_options))
            {
                var repository = new AhmedOumezzine.EFCore.Repository.Repository.Repository<MyDbContext>(context);
                var entity = new MyEntity { Name = "demo" };

                context.MyEntities.Add(entity);
                await context.SaveChangesAsync();

                // Act
                var retrievedEntity = await repository.GetByIdAsync<MyEntity>(entity.Id, asNoTracking: true);

                // Assert
                Assert.IsNotNull(retrievedEntity);
                Assert.AreEqual(EntityState.Detached, context.Entry(retrievedEntity).State);
            }
        }

        [TestMethod]
        public async Task GetByIdAsync_WithProjection_ShouldReturnProjectedData()
        {
            // Arrange
            using (var context = new MyDbContext(_options))
            {
                var repository = new AhmedOumezzine.EFCore.Repository.Repository.Repository<MyDbContext>(context);
                var entity = new MyEntity { Name = "demo" };

                context.MyEntities.Add(entity);
                await context.SaveChangesAsync();

                // Act
                var projectedData = await repository.GetByIdAsync<MyEntity, string>(entity.Id, e => e.Name);

                // Assert
                Assert.AreEqual("demo", projectedData);
            }
        }
    }
}