using AhmedOumezzine.EFCore.Repository.Entities;
using AhmedOumezzine.EFCore.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace AhmedOumezzine.EFCore.Repository.Repository
{
    /// <summary>
    /// Partial implementation of the generic repository for updating entities.
    /// Provides methods to update single or multiple entities, partial updates, and safe operations.
    /// </summary>
    /// <typeparam name="TDbContext">The type of the database context.</typeparam>
    public sealed partial class Repository<TDbContext> : IRepository
        where TDbContext : DbContext
    {
        #region Update (Sync)

        /// <summary>
        /// Updates the specified entity in the database.
        /// If the entity is already tracked, marks it as modified.
        /// If not tracked, attaches it and marks it as modified (requires valid primary key).
        /// </summary>
        /// <typeparam name="TEntity">The entity type, must inherit from <see cref="BaseEntity"/>.</typeparam>
        /// <param name="entity">The entity to update. Cannot be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="entity"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the entity has an invalid or default primary key value.</exception>
        public void Update<TEntity>(TEntity entity) where TEntity : BaseEntity
        {
            CheckEntityIsNull(entity);

            var entry = _dbContext.ChangeTracker.Entries<TEntity>()
                .FirstOrDefault(e => e.Entity == entity);

            if (entry != null)
            {
                entry.State = EntityState.Modified;
            }
            else
            {
                if (!HasValidKey(entity))
                {
                    throw new InvalidOperationException(
                        "Cannot update an entity with an invalid or default primary key value.");
                }

                SetLastModifiedOnUtc(entity);
                _dbContext.Entry(entity).State = EntityState.Modified;
            }
        }

        /// <summary>
        /// Updates a collection of entities in the database.
        /// Each entity is processed individually to ensure proper state management.
        /// </summary>
        /// <typeparam name="TEntity">The entity type, must inherit from <see cref="BaseEntity"/>.</typeparam>
        /// <param name="entities">The collection of entities to update. Cannot be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="entities"/> is null.</exception>
        public void Update<TEntity>(IEnumerable<TEntity> entities) where TEntity : BaseEntity
        {
            CheckEntitiesIsNull(entities);

            foreach (var entity in entities)
            {
                Update(entity);
            }
        }

        #endregion Update (Sync)

        #region Update (Async)

        /// <summary>
        /// Asynchronously updates the specified entity and saves changes.
        /// </summary>
        /// <typeparam name="TEntity">The entity type, must inherit from <see cref="BaseEntity"/>.</typeparam>
        /// <param name="entity">The entity to update. Cannot be null.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The number of affected rows.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="entity"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the entity has an invalid or default primary key value.</exception>
        public async Task<int> UpdateAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            CheckEntityIsNull(entity);

            var entry = _dbContext.ChangeTracker.Entries<TEntity>()
                .FirstOrDefault(e => e.Entity == entity);

            if (entry != null)
            {
                entry.State = EntityState.Modified;
            }
            else
            {
                if (!HasValidKey(entity))
                {
                    throw new InvalidOperationException(
                        "Cannot update an entity with an invalid or default primary key value.");
                }

                SetLastModifiedOnUtc(entity);
                _dbContext.Entry(entity).State = EntityState.Modified;
            }

            return await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously updates a collection of entities and saves changes.
        /// </summary>
        /// <typeparam name="TEntity">The entity type, must inherit from <see cref="BaseEntity"/>.</typeparam>
        /// <param name="entities">The collection of entities to update. Cannot be null.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The number of affected rows.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="entities"/> is null.</exception>
        public async Task<int> UpdateAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            CheckEntitiesIsNull(entities);

            foreach (var entity in entities)
            {
                Update(entity);
            }

            return await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        #endregion Update (Async)

        #region Update Only (Partial Update)

        /// <summary>
        /// Updates only the specified properties of the entity.
        /// Useful for PATCH operations in APIs to avoid over-posting.
        /// </summary>
        /// <typeparam name="TEntity">The entity type, must inherit from <see cref="BaseEntity"/>.</typeparam>
        /// <param name="entity">The entity with updated values. Must have a valid primary key.</param>
        /// <param name="properties">The names of properties to update (e.g., "Name", "Email").</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The number of affected rows.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="entity"/> or <paramref name="properties"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the entity has an invalid primary key.</exception>
        public async Task<int> UpdateOnlyAsync<TEntity>(
         TEntity entity,
         string[] properties,
         CancellationToken cancellationToken = default)
         where TEntity : BaseEntity
        {
            CheckEntityIsNull(entity);
            if (properties == null) throw new ArgumentNullException(nameof(properties));

            if (!HasValidKey(entity))
                throw new InvalidOperationException("Entity must have a valid primary key to perform partial update.");

            // Attacher l'entité si elle n'est pas suivie
            var entry = _dbContext.Entry(entity);
            if (entry.State == EntityState.Detached)
            {
                _dbContext.Set<TEntity>().Attach(entity);
                entry.State = EntityState.Unchanged;
            }

            foreach (var prop in properties)
            {
                var propertyEntry = entry.Property(prop);
                if (propertyEntry != null && !propertyEntry.Metadata.IsKey())
                {
                    propertyEntry.IsModified = true;
                }
            }

            SetLastModifiedOnUtc(entity);
            return await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }


        #endregion Update Only (Partial Update)

        #region Conditional Update

        /// <summary>
        /// Updates the entity only if it exists in the database.
        /// </summary>
        /// <typeparam name="TEntity">The entity type, must inherit from <see cref="BaseEntity"/>.</typeparam>
        /// <param name="entity">The entity to update. Cannot be null.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>True if the entity was found and updated; false otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="entity"/> is null.</exception>
        public async Task<bool> UpdateIfExistsAsync<TEntity>(
         TEntity entity,
         CancellationToken cancellationToken = default)
         where TEntity : BaseEntity
        {
            CheckEntityIsNull(entity);

            if (!HasValidKey(entity)) return false;

            // Récupérer la propriété clé
            var keyProperty = _dbContext.Model.FindEntityType(typeof(TEntity))
                ?.FindPrimaryKey()?.Properties.FirstOrDefault();

            if (keyProperty == null)
                throw new InvalidOperationException("Entity does not have a primary key.");

            var keyValue = typeof(TEntity).GetProperty(keyProperty.Name).GetValue(entity);

            // Utiliser EF.Property pour que la requête soit traduisible en SQL
            var exists = await _dbContext.Set<TEntity>()
                .AnyAsync(e => EF.Property<object>(e, keyProperty.Name).Equals(keyValue), cancellationToken);

            if (!exists) return false;

            Update(entity);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return true;
        }


        #endregion Conditional Update

        #region Safe Update

        /// <summary>
        /// Attempts to update the entity and returns success status without throwing exceptions.
        /// Useful for batch operations where individual failures should not stop the process.
        /// </summary>
        /// <typeparam name="TEntity">The entity type, must inherit from <see cref="BaseEntity"/>.</typeparam>
        /// <param name="entity">The entity to update.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>True if update succeeded; false if an exception occurred.</returns>
        public async Task<bool> TryUpdateAsync<TEntity>(
            TEntity entity,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            try
            {
                await UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion Safe Update

        #region Bulk Update (EF Core 7+)

        /// <summary>
        /// Updates entities matching the predicate without loading them into memory.
        /// Uses EF Core 7+ ExecuteUpdate feature for high performance.
        /// Note: Does not trigger change tracking or events.
        /// </summary>
        /// <typeparam name="TEntity">The entity type, must inherit from <see cref="BaseEntity"/>.</typeparam>
        /// <param name="predicate">The condition to match entities (e.g., x => x.Status == "Pending").</param>
        /// <param name="updateAction">The update operation (e.g., x => x.SetProperty(u => u.Status, "Processed")).</param>
        /// <returns>The number of affected rows.</returns>
        public async Task<int> UpdateFromQueryAsync<TEntity>(
            Expression<Func<TEntity, bool>> predicate,
            Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> updateAction)
            where TEntity : BaseEntity
        {
            // Apply the user update
            var result = await _dbContext.Set<TEntity>()
                .Where(predicate)
                .ExecuteUpdateAsync(updateAction);

            // Also update LastModifiedOnUtc
            await _dbContext.Set<TEntity>()
                .Where(predicate)
                .ExecuteUpdateAsync(e => e.SetProperty(x => x.LastModifiedOnUtc, DateTime.UtcNow));

            return result;
        }

        #endregion Bulk Update (EF Core 7+)
    }
}