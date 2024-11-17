using AhmedOumezzine.EFCore.Repository.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AhmedOumezzine.EFCore.Repository.Tests.Repository
{
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
        {
        }

        public DbSet<MyEntity> MyEntities { get; set; }

        // Autres DbSets pour vos autres entités

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuration des modèles d'entité
            modelBuilder.Entity<MyEntity>()
                .HasKey(e => e.Id);
            // Ajoutez d'autres configurations d'entité selon vos besoins

            base.OnModelCreating(modelBuilder);
        }
    }

    public class MyEntity : BaseEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int RelatedEntityId { get; set; }
        public MyRelatedEntity RelatedEntity { get; set; }
    }

    public class MyRelatedEntity : BaseEntity
    {
        public int Id { get; set; }
        public string RelatedData { get; set; }
        public ICollection<MyEntity> MyEntities { get; set; }
    }

    [TestClass]
    public class AddRepositoryTest
    {
        private DbContextOptions<MyDbContext> _options;

        [TestInitialize]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<MyDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
        }

        [TestMethod]
        public void Add_ValidEntity_ShouldAddToDatabase()
        {
            // Arrange
            using (var context = new MyDbContext(_options))
            {
                var repository = new AhmedOumezzine.EFCore.Repository.Repository.Repository<MyDbContext>(context);
                var entity = new MyEntity { Name = "DEmo" };

                // Act
                repository.Add(entity);
                context.SaveChanges();

                // Assert
                Assert.AreEqual(1, context.MyEntities.Count()); // Vérifie que l'entité a été ajoutée à la base de données
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Add_NullEntity_ShouldThrowArgumentNullException()
        {
            // Arrange
            using (var context = new MyDbContext(_options))
            {
                var repository = new AhmedOumezzine.EFCore.Repository.Repository.Repository<MyDbContext>(context);
                MyEntity entity = null;
                // Act
                repository.Add(entity); // Doit lever ArgumentNullException
            }

            // Assert
            // L'exception est gérée par l'attribut ExpectedException
        }

        [TestMethod]
        public async Task AddAsync_ValidEntity_ShouldAddToDatabase()
        {
            // Arrange
            using (var context = new MyDbContext(_options))
            {
                var repository = new AhmedOumezzine.EFCore.Repository.Repository.Repository<MyDbContext>(context);
                var entity = new MyEntity { Name = "demo" };

                // Act
                await repository.AddAsync(entity);
                await context.SaveChangesAsync();

                // Assert
                Assert.AreEqual(1, context.MyEntities.Count()); // Vérifie que l'entité a été ajoutée à la base de données
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AddAsync_NullEntity_ShouldThrowArgumentNullException()
        {
            // Arrange
            using (var context = new MyDbContext(_options))
            {
                var repository = new AhmedOumezzine.EFCore.Repository.Repository.Repository<MyDbContext>(context);
                IEnumerable<MyEntity> entities = null;

                // Act
                await repository.AddAsync(entities); // Doit lever ArgumentNullException
            }

            // Assert
            // L'exception est gérée par l'attribut ExpectedException
        }

        [TestMethod]
        public async Task InsertAsync_ValidEntity_ShouldReturnPrimaryKeyValues()
        {
            // Arrange
            using (var context = new MyDbContext(_options))
            {
                var repository = new AhmedOumezzine.EFCore.Repository.Repository.Repository<MyDbContext>(context);
                var entity = new MyEntity { Name = "demo" };

                // Act
                var primaryKeyValues = await repository.InsertAsync(entity);
                await context.SaveChangesAsync();

                // Assert
                Assert.IsNotNull(primaryKeyValues); // Vérifie que les valeurs de clé primaire retournées ne sont pas nulles
                                                    // Ajoutez d'autres assertions en fonction de vos besoins
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task InsertAsync_NullEntity_ShouldThrowArgumentNullException()
        {
            // Arrange
            using (var context = new MyDbContext(_options))
            {
                var repository = new AhmedOumezzine.EFCore.Repository.Repository.Repository<MyDbContext>(context);
                MyEntity entity = null;

                // Act
                await repository.InsertAsync(entity); // Doit lever ArgumentNullException
            }

            // Assert
            // L'exception est gérée par l'attribut ExpectedException
        }

        [TestMethod]
        public async Task InsertAsync_ValidEntities_ShouldInsertToDatabase()
        {
            // Arrange
            using (var context = new MyDbContext(_options))
            {
                var repository = new AhmedOumezzine.EFCore.Repository.Repository.Repository<MyDbContext>(context);
                var entities = new List<MyEntity>
            {
                new MyEntity { Name= "demo" },
                new MyEntity { Name= "demo2" },
            };

                // Act
                await repository.InsertAsync<MyEntity>(entities);
                await context.SaveChangesAsync();

                // Assert
                Assert.AreEqual(2, context.MyEntities.Count()); // Vérifie que les entités ont été ajoutées à la base de données
                                                                // Ajoutez d'autres assertions en fonction de vos besoins
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task InsertAsync_NullEntities_ShouldThrowArgumentNullException()
        {
            // Arrange
            using (var context = new MyDbContext(_options))
            {
                var repository = new AhmedOumezzine.EFCore.Repository.Repository.Repository<MyDbContext>(context);
                IEnumerable<MyEntity> entities = null;
                // Act
                await repository.InsertAsync(entities); // Doit lever ArgumentNullException
            }

            // Assert
            // L'exception est gérée par l'attribut ExpectedException
        }
    }
}