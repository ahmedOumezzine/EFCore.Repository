using AhmedOumezzine.EFCore.Repository.Entities;
using AhmedOumezzine.EFCore.Repository.Repository;
using AhmedOumezzine.EFCore.Tests.Entity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SQLitePCL;

namespace AhmedOumezzine.EFCore.Repository.Tests
{
    /// <summary>
    /// Classe de base pour les tests d'intégration avec SQLite en mémoire.
    /// ATTENTION : Chaque classe de test concrète DOIT appeler InitializeDatabase() dans son [ClassInitialize].
    /// </summary>
    public abstract class RepositoryTestBase<TEntity>
        where TEntity : BaseEntity, new()
    {
        private static SqliteConnection? _connection;
        protected static DbContextOptions<TestDbContext>? _options;

        /// <summary>
        /// À appeler dans [ClassInitialize] de chaque classe de test concrète.
        /// </summary>
        public static void InitializeDatabase()
        {
            if (_options != null) return; // Déjà initialisé

            // 🔑 Obligatoire pour .NET 9
            Batteries_V2.Init();

            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            _options = new DbContextOptionsBuilder<TestDbContext>()
                .UseSqlite(_connection)
                .Options;

            using var dbContext = new TestDbContext(_options);
            dbContext.Database.EnsureCreated();
        }

        /// <summary>
        /// À appeler dans [ClassCleanup] de chaque classe de test concrète.
        /// </summary>
        public static void CleanupDatabase()
        {
            _connection?.Close();
            _connection?.Dispose();
            _connection = null;
            _options = null;
        }

        /// <summary>
        /// Crée une nouvelle instance de DbContext.
        /// </summary>
        protected static TestDbContext CreateDbContext()
        {
            // 🔁 Initialisation de secours (au cas où ClassInitialize n'aurait pas été appelé)
            if (_options == null)
            {
                InitializeDatabase();
            }
            return new TestDbContext(_options!);
        }

        /// <summary>
        /// Crée une nouvelle instance du Repository.
        /// </summary>
        protected static Repository<TestDbContext> CreateRepository()
        {
            return new Repository<TestDbContext>(CreateDbContext());
        }

        /// <summary>
        /// Réinitialise la base de données (à appeler dans [TestInitialize]).
        /// </summary>
        protected static void RecreateDatabase()
        {
            using var context = CreateDbContext();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }
    }

    // ========================
    // DbContext et Entité de test
    // ========================

    public class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

        public DbSet<TestEntity> TestEntities => Set<TestEntity>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestEntity>(entity =>
            {
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                // Ajoute une contrainte CHECK pour SQLite
                entity.HasCheckConstraint("CK_TestEntity_Name_Length", "LENGTH(Name) <= 100");
                entity.Property(e => e.Description).HasMaxLength(500);

                // Ajoute une contrainte CHECK pour SQLite
                entity.HasCheckConstraint("CK_TestEntity_Description_Length", "LENGTH(Description) <= 500");
            });
        }
    }

    
}