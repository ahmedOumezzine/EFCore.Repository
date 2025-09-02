using AhmedOumezzine.EFCore.Repository.Entities;
using AhmedOumezzine.EFCore.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AhmedOumezzine.EFCore.Repository.Repository
{
    /// <summary>
    /// Partial implementation of the generic repository for deleting entities.
    /// Supports soft delete, hard delete, conditional operations, and bulk actions.
    /// </summary>
    /// <typeparam name="TDbContext">The type of the database context.</typeparam>
    public sealed partial class Repository<TDbContext> : IRepository
        where TDbContext : DbContext
    {
        #region Soft Delete (Mark as Deleted)

        /// <summary>
        /// Marks an entity as deleted (soft delete). Does NOT save changes.
        /// Sets <see cref="BaseEntity.IsDeleted"/> to true and <see cref="BaseEntity.DeletedOnUtc"/> to current UTC time.
        /// </summary>
        /// <typeparam name="TEntity">The entity type, must inherit from <see cref="BaseEntity"/>.</typeparam>
        /// <param name="entity">The entity to mark as deleted. Cannot be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="entity"/> is null.</exception>
        public void Remove<TEntity>(TEntity entity) where TEntity : BaseEntity
        {
            CheckEntityIsNull(entity);
            SetDeleted(entity);
            _dbContext.Entry(entity).State = EntityState.Modified;
        }

        /// <summary>
        /// Marks a collection of entities as deleted (soft delete). Does NOT save changes.
        /// </summary>
        /// <typeparam name="TEntity">The entity type, must inherit from <see cref="BaseEntity"/>.</typeparam>
        /// <param name="entities">The collection of entities to mark as deleted. Cannot be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="entities"/> is null.</exception>
        public void Remove<TEntity>(IEnumerable<TEntity> entities) where TEntity : BaseEntity
        {
            CheckEntitiesIsNull(entities);
            SetDeleted(entities);
            foreach (var entity in entities)
            {
                _dbContext.Entry(entity).State = EntityState.Modified;
            }
        }

        /// <summary>
        /// Asynchronously marks an entity as deleted and saves changes.
        /// </summary>
        /// <typeparam name="TEntity">The entity type, must inherit from <see cref="BaseEntity"/>.</typeparam>
        /// <param name="entity">The entity to delete. Cannot be null.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The number of affected rows.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="entity"/> is null.</exception>
        public async Task<int> DeleteAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            CheckEntityIsNull(entity);
            SetDeleted(entity);
            _dbContext.Entry(entity).State = EntityState.Modified;
            return await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously marks a collection of entities as deleted and saves changes.
        /// </summary>
        /// <typeparam name="TEntity">The entity type, must inherit from <see cref="BaseEntity"/>.</typeparam>
        /// <param name="entities">The collection to delete. Cannot be null.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The number of affected rows.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="entities"/> is null.</exception>
        public async Task<int> DeleteAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            CheckEntitiesIsNull(entities);
            SetDeleted(entities);
            foreach (var entity in entities)
            {
                _dbContext.Entry(entity).State = EntityState.Modified;
            }
            return await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        #endregion Soft Delete (Mark as Deleted)

        #region Hard Delete (Physical Removal)

        /// <summary>
        /// Permanently removes an entity from the database (hard delete).
        /// Bypasses soft delete and performs a physical DELETE.
        /// </summary>
        /// <typeparam name="TEntity">The entity type, must inherit from <see cref="BaseEntity"/>.</typeparam>
        /// <param name="entity">The entity to remove. Cannot be null.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The number of affected rows.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="entity"/> is null.</exception>
        public async Task<int> HardDeleteAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            CheckEntityIsNull(entity);
            _dbContext.Set<TEntity>().Remove(entity);
            return await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Permanently removes a collection of entities from the database (hard delete).
        /// </summary>
        /// <typeparam name="TEntity">The entity type, must inherit from <see cref="BaseEntity"/>.</typeparam>
        /// <param name="entities">The collection to remove. Cannot be null.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The number of affected rows.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="entities"/> is null.</exception>
        public async Task<int> HardDeleteAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            CheckEntitiesIsNull(entities);
            _dbContext.Set<TEntity>().RemoveRange(entities);
            return await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        #endregion Hard Delete (Physical Removal)

        #region Conditional Delete

        /// <summary>
        /// Deletes an entity only if it exists in the database.
        /// </summary>
        /// <typeparam name="TEntity">The entity type, must inherit from <see cref="BaseEntity"/>.</typeparam>
        /// <param name="entity">The entity to delete. Cannot be null.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>True if the entity was found and deleted; false otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="entity"/> is null.</exception>
        public async Task<bool> DeleteIfExistsAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            CheckEntityIsNull(entity);

            var key = GetKeyValue(entity);
            var exists = await _dbContext.Set<TEntity>().AnyAsync(e => GetKeyValue(e).Equals(key), cancellationToken);

            if (!exists) return false;

            await DeleteAsync(entity, cancellationToken);
            return true;
        }

        #endregion Conditional Delete

        #region Safe Delete

        /// <summary>
        /// Attempts to delete an entity and returns success status without throwing.
        /// Useful for batch operations.
        /// </summary>
        /// <typeparam name="TEntity">The entity type, must inherit from <see cref="BaseEntity"/>.</typeparam>
        /// <param name="entity">The entity to delete.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>True if deletion succeeded; false if an exception occurred.</returns>
        public async Task<bool> TryDeleteAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            try
            {
                await DeleteAsync(entity, cancellationToken).ConfigureAwait(false);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion Safe Delete

        #region Bulk Delete (EF Core 7+)

        /// <summary>
        /// Deletes entities matching the predicate without loading them into memory.
        /// Uses EF Core 7+ ExecuteDelete feature for high performance.
        /// Note: Does not trigger change tracking or events.
        /// </summary>
        /// <typeparam name="TEntity">The entity type, must inherit from <see cref="BaseEntity"/>.</typeparam>
        /// <param name="predicate">The condition to match (e.g., x => x.IsDeleted && x.DeletedOnUtc < DateTime.UtcNow.AddMonths(-6)).</param>
        /// <returns>The number of affected rows.</returns>
        public async Task<int> DeleteFromQueryAsync<TEntity>(Expression<Func<TEntity, bool>> predicate)
            where TEntity : BaseEntity
        {
            return await _dbContext.Set<TEntity>()
                .Where(predicate)
                .ExecuteDeleteAsync();
        }

        #endregion Bulk Delete (EF Core 7+)

        #region Restore (Undelete)

        /// <summary>
        /// Restores a soft-deleted entity by setting IsDeleted to false and clearing DeletedOnUtc.
        /// </summary>
        /// <typeparam name="TEntity">The entity type, must inherit from <see cref="BaseEntity"/>.</typeparam>
        /// <param name="entity">The entity to restore.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The number of affected rows.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="entity"/> is null.</exception>
        public async Task<int> RestoreAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            CheckEntityIsNull(entity);

            if (!entity.IsDeleted) return 0; // Already active

            entity.IsDeleted = false;
            entity.DeletedOnUtc = null;

            _dbContext.Entry(entity).State = EntityState.Modified;
            return await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        #endregion Restore (Undelete)

        #region Delete by Condition (Soft)

        /// <summary>
        /// Soft deletes all entities matching the predicate.
        /// </summary>
        /// <typeparam name="TEntity">The entity type, must inherit from <see cref="BaseEntity"/>.</typeparam>
        /// <param name="predicate">The condition (e.g., x => x.CreatedOnUtc < threshold).</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The number of entities marked as deleted.</returns>
        public async Task<int> DeleteRangeByConditionAsync<TEntity>(
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            var entities = await _dbContext.Set<TEntity>()
                .Where(predicate)
                .Where(e => !e.IsDeleted)
                .ToListAsync(cancellationToken);

            if (!entities.Any()) return 0;

            SetDeleted(entities);
            foreach (var entity in entities)
            {
                _dbContext.Entry(entity).State = EntityState.Modified;
            }

            return await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        #endregion Delete by Condition (Soft)
    }
}