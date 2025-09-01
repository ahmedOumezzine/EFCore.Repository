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


        public async Task<IDbContextTransaction> BeginTransactionAsync(
            IsolationLevel isolationLevel = IsolationLevel.Unspecified,
            CancellationToken cancellationToken = default)
        {
            IDbContextTransaction dbContextTransaction = await _dbContext.Database.BeginTransactionAsync(isolationLevel, cancellationToken);
            return dbContextTransaction;
        }

        public IQueryable<T> GetQueryable<T>() where T : BaseEntity
        {
            return _dbContext.Set<T>();
        }

        public void ClearChangeTracker()
        {
            _dbContext.ChangeTracker.Clear();
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            int count = await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return count;
        }

        /// <summary>
        /// Ensures the entity is not null.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="entity">The entity to check.</param>
        /// <exception cref="ArgumentNullException"></exception>
        private static void CheckEntityIsNull<TEntity>(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity), $"The entity of type {typeof(TEntity).Name} cannot be null.");
        }

        /// <summary>
        /// Ensures the collection of entities is not null or empty.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="entities">The collection to check.</param>
        /// <exception cref="ArgumentNullException"></exception>
        private static void CheckEntitiesIsNull<TEntity>(IEnumerable<TEntity> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities), $"The collection of entities of type {typeof(TEntity).Name} cannot be null.");

            // Optional: if you want to prevent empty collections from being passed
            // if (!entities.Any()) throw new ArgumentException("The collection cannot be empty.", nameof(entities));
        }

        /// <summary>
        /// Sets the CreatedOnUtc property if not already set.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="entity">The entity to update.</param>
        private void SetCreatedOnUtc<TEntity>(TEntity entity) where TEntity : BaseEntity
        {
            if (entity.CreatedOnUtc == default)
                entity.CreatedOnUtc = DateTime.UtcNow;
        }

        /// <summary>
        /// Sets the CreatedOnUtc property for a collection of entities if not already set.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="entities">The collection of entities.</param>
        private void SetCreatedOnUtc<TEntity>(IEnumerable<TEntity> entities) where TEntity : BaseEntity
        {
            foreach (var entity in entities)
            {
                if (entity.CreatedOnUtc == default)
                    entity.CreatedOnUtc = DateTime.UtcNow;
            }
        }
         
        private bool HasValidKey<TEntity>(TEntity entity) where TEntity : BaseEntity
        {
            var entityType = _dbContext.Model.FindEntityType(typeof(TEntity));
            var primaryKey = entityType?.FindPrimaryKey();

            if (primaryKey == null) return false;

            foreach (var property in primaryKey.Properties)
            {
                var propertyInfo = typeof(TEntity).GetProperty(property.Name);
                var value = propertyInfo?.GetValue(entity);
                var defaultValue = property.ClrType.IsValueType
                    ? Activator.CreateInstance(property.ClrType)
                    : null;

                if (value == null || value.Equals(defaultValue))
                    return false;
            }

            return true;
        }

        private void SetLastModifiedOnUtc<TEntity>(TEntity entity) where TEntity : BaseEntity
        {
            entity.LastModifiedOnUtc = DateTime.UtcNow;
        }

        private void SetLastModifiedOnUtc<TEntity>(IEnumerable<TEntity> entities) where TEntity : BaseEntity
        {
            foreach (var entity in entities)
            {
                entity.LastModifiedOnUtc = DateTime.UtcNow;
            }
        }



        // Méthode pour définir IsDeleted sur true et DeletedOnUtc pour une seule entité
         
        private void SetDeleted<TEntity>(TEntity entity) where TEntity : BaseEntity
        {
            if (!entity.IsDeleted)
            {
                entity.IsDeleted = true;
                entity.DeletedOnUtc = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Marks a collection of entities as deleted.
        /// </summary>
        private void SetDeleted<TEntity>(IEnumerable<TEntity> entities) where TEntity : BaseEntity
        {
            foreach (var entity in entities)
            {
                if (!entity.IsDeleted)
                {
                    entity.IsDeleted = true;
                    entity.DeletedOnUtc = DateTime.UtcNow;
                }
            }
        }


        /// <summary>
        /// Gets the primary key value of the entity.
        /// </summary>

        private object GetKeyValue<TEntity>(TEntity entity)
        {
            var entityType = _dbContext.Model.FindEntityType(typeof(TEntity));
            var key = entityType?.FindPrimaryKey().Properties.First();
            return key != null ? typeof(TEntity).GetProperty(key.Name).GetValue(entity) : null;
        }

    }
}