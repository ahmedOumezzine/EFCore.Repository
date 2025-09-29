using AhmedOumezzine.EFCore.Repository.Entities;
using AhmedOumezzine.EFCore.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;
using System.Diagnostics;

namespace AhmedOumezzine.EFCore.Repository.Repository
{
    [DebuggerStepThrough]
    public sealed partial class Repository<TDbContext> : IRepository
        where TDbContext : DbContext
    {
        /// <summary>
        /// The database context used for data operations.
        /// </summary>
        private readonly TDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="Repository{TDbContext}"/> class.
        /// </summary>
        /// <param name="dbContext">The database context. Cannot be null.</param>
        public Repository(TDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        #region Helper

        /// <summary>
        /// Prepares a BaseEntity for insertion by setting audit and identity properties.
        /// </summary>
        private static void PrepareEntityForInsert<TEntity>(TEntity entity)
            where TEntity : BaseEntity
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            var now = DateTime.UtcNow;

            if (entity.Id == Guid.Empty)
                entity.Id = Guid.NewGuid();

            entity.CreatedOnUtc = now;
            entity.LastModifiedOnUtc = now;
            entity.IsDeleted = false;
            entity.DeletedOnUtc = null;
        }

        #endregion Helper

        #region Helper

        /// <summary>
        /// Marks an entity as soft-deleted with audit timestamp.
        /// </summary>
        private static void MarkAsDeleted<TEntity>(TEntity entity) where TEntity : BaseEntity
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (entity.IsDeleted) return; // déjà supprimé → rien à faire

            entity.IsDeleted = true;
            entity.DeletedOnUtc = DateTime.UtcNow;
        }

        /// <summary>
        /// Marks a collection of entities as soft-deleted.
        /// </summary>
        private static void MarkAsDeleted<TEntity>(IEnumerable<TEntity> entities) where TEntity : BaseEntity
        {
            if (entities == null) throw new ArgumentNullException(nameof(entities));
            foreach (var entity in entities)
                MarkAsDeleted(entity);
        }

        #endregion Helper
        #region Helper

        /// <summary>
        /// Sets the LastModifiedOnUtc property to current UTC time.
        /// </summary>
        private static void SetLastModifiedOnUtc<TEntity>(TEntity entity) where TEntity : BaseEntity
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            entity.LastModifiedOnUtc = DateTime.UtcNow;
        }

        #endregion

 
         
    
 
       
    }
}