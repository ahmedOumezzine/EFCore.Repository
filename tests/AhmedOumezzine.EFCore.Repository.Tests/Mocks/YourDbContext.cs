using AhmedOumezzine.EFCore.Repository.Tests.Models;
using Microsoft.EntityFrameworkCore;

namespace Repository.Tests
{
    /// <summary>
    /// Contexte de test utilisé uniquement pour les tests unitaires.
    /// Simule un DbContext EF Core avec des entités héritant de BaseEntity.
    /// </summary>
    public class YourDbContext : DbContext
    {
        public YourDbContext()
        { }

        public YourDbContext(DbContextOptions<YourDbContext> options) : base(options)
        {
        }

        // Ajoute ici tous les DbSet<T> dont tu as besoin dans les tests
        public virtual DbSet<Product> Products { get; set; }

        public virtual DbSet<Category> Categories { get; set; }

        // Important : Configure pour que EF ne tente pas de créer des clés étrangères
        // utile quand on mocke et qu'on n'a pas de base réelle
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Désactiver les contraintes de suppression en cascade
            foreach (var relationship in modelBuilder.Model.GetEntityTypes()
                .SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.NoAction;
            }

            // Appliquer les configurations si tu en as (optionnel)
            // modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            base.OnModelCreating(modelBuilder);
        }
    }
}