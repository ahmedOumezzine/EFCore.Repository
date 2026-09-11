using AhmedOumezzine.EFCore.Repository.Entities;
using AhmedOumezzine.EFCore.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AhmedOumezzine.EFCore.Repository.Repository
{
    /// <summary>
    /// Partial implementation of the generic repository for deleting entities.
    /// Supports soft delete, hard delete, conditional operations, bulk actions, and restore.
    /// </summary>
    public sealed partial class Repository<TDbContext> : IRepository
        where TDbContext : DbContext
    {
        #region Soft Delete (Mark as Deleted)

        /// <summary>
        /// Marks an entity as deleted (soft delete). Does NOT save changes.
        /// </summary>
        /// <inheritdoc />
        public void Remove<TEntity>(TEntity entity) where TEntity : BaseEntity
        {
            MarkAsDeleted(entity);
            MarkSoftDeleteProperties(entity);
        }

        /// <summary>
        /// Marks a collection of entities as deleted (soft delete). Does NOT save changes.
        /// </summary>
        /// <inheritdoc />
        public void Remove<TEntity>(IEnumerable<TEntity> entities) where TEntity : BaseEntity
        {
            MarkAsDeleted(entities);
            foreach (var entity in entities)
                MarkSoftDeleteProperties(entity);
        }

        /// <summary>
        /// Synchronously soft-deletes an entity and saves changes.
        /// </summary>
        /// <inheritdoc />
        public int Delete<TEntity>(TEntity entity) where TEntity : BaseEntity
        {
            MarkAsDeleted(entity);
            MarkSoftDeleteProperties(entity);
            return _dbContext.SaveChanges();
        }

        /// <summary>
        /// Synchronously soft-deletes a collection and saves changes.
        /// </summary>
        /// <inheritdoc />
        public int Delete<TEntity>(IEnumerable<TEntity> entities) where TEntity : BaseEntity
        {
            MarkAsDeleted(entities);
            foreach (var entity in entities)
                MarkSoftDeleteProperties(entity);
            return _dbContext.SaveChanges();
        }

        /// <summary>
        /// Asynchronously soft-deletes an entity and saves changes.
        /// </summary>
        /// <inheritdoc />
        public async Task<int> DeleteAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            MarkAsDeleted(entity);
            MarkSoftDeleteProperties(entity);
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Asynchronously soft-deletes a collection and saves changes.
        /// </summary>
        /// <inheritdoc />
        public async Task<int> DeleteAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            MarkAsDeleted(entities);
            foreach (var entity in entities)
                MarkSoftDeleteProperties(entity);
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }

        #endregion Soft Delete (Mark as Deleted)

        #region Hard Delete (Physical Removal)

        /// <inheritdoc />
        public async Task<int> HardDeleteAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            _dbContext.Set<TEntity>().Remove(entity);
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<int> HardDeleteAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            if (entities == null) throw new ArgumentNullException(nameof(entities));
            _dbContext.Set<TEntity>().RemoveRange(entities);
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }

        #endregion Hard Delete (Physical Removal)

        #region Conditional & Safe Delete

        /// <inheritdoc />
        public async Task<bool> DeleteIfExistsAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            var exists = await _dbContext.Set<TEntity>().AnyAsync(e => e.Id == entity.Id, cancellationToken);
            if (!exists) return false;

            await DeleteAsync(entity, cancellationToken);
            return true;
        }

        /// <inheritdoc />
        public async Task<bool> DeleteByIdAsync<TEntity>(object id, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            var entity = await _dbContext.Set<TEntity>().IgnoreQueryFilters().FirstOrDefaultAsync(e => e.Id == (Guid)id, cancellationToken);
            if (entity == null || entity.IsDeleted) return false;

            MarkAsDeleted(entity);
            MarkSoftDeleteProperties(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }

        /// <inheritdoc />
        public async Task<bool> TryDeleteAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            try
            {
                await DeleteAsync(entity, cancellationToken);
                return true;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
                return false;
            }
        }

        #endregion Conditional & Safe Delete

        #region Bulk Delete (EF Core 7+)

        /// <summary>
        /// Hard delete via bulk (no tracking).
        /// </summary>
        /// <inheritdoc />
        public async Task<int> DeleteFromQueryAsync<TEntity>(Expression<Func<TEntity, bool>> predicate)
            where TEntity : BaseEntity
        {
            return await _dbContext.Set<TEntity>().Where(predicate).ExecuteDeleteAsync();
        }

        /// <summary>
        /// Soft delete via bulk update (EF Core 7+).
        /// </summary>
        /// <inheritdoc />
        public async Task<int> SoftDeleteFromQueryAsync<TEntity>(Expression<Func<TEntity, bool>> predicate)
            where TEntity : BaseEntity
        {
            return await _dbContext.Set<TEntity>()
                .Where(predicate)
                .Where(e => !e.IsDeleted)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(e => e.IsDeleted, true)
                    .SetProperty(e => e.DeletedOnUtc, DateTime.UtcNow));
        }

        #endregion Bulk Delete (EF Core 7+)

        #region Restore (Undelete)

        /// <inheritdoc />
        public async Task<int> RestoreAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (!entity.IsDeleted) return 0;

            var tracked = await _dbContext.Set<TEntity>().IgnoreQueryFilters()
                .FirstOrDefaultAsync(e => e.Id == entity.Id && e.IsDeleted, cancellationToken);
            if (tracked == null) return 0;
            tracked.IsDeleted = false;
            tracked.DeletedOnUtc = null;
            tracked.LastModifiedOnUtc = DateTime.UtcNow;
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<int> RestoreRangeAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            var toRestore = entities?.Where(e => e.IsDeleted).ToList() ?? new List<TEntity>();
            if (!toRestore.Any()) return 0;

            foreach (var entity in toRestore)
            {
                entity.IsDeleted = false;
                entity.DeletedOnUtc = null;
                MarkSoftDeleteProperties(entity);
            }
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<bool> RestoreByIdAsync<TEntity>(object id, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            var affected = await _dbContext.Set<TEntity>().IgnoreQueryFilters()
                .Where(e => e.Id == (Guid)id && e.IsDeleted)
                .ExecuteUpdateAsync(s => s.SetProperty(e => e.IsDeleted, false)
                    .SetProperty(e => e.DeletedOnUtc, (DateTime?)null)
                    .SetProperty(e => e.LastModifiedOnUtc, DateTime.UtcNow), cancellationToken);
            return affected > 0;
        }

        /// <inheritdoc />
        public async Task<bool> TryRestoreAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            try
            {
                await RestoreAsync(entity, cancellationToken);
                return true;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
                return false;
            }
        }

        #endregion Restore (Undelete)

        #region Delete by Condition & Purge

        /// <inheritdoc />
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

            MarkAsDeleted(entities);
            foreach (var entity in entities)
                MarkSoftDeleteProperties(entity);

            return await _dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Purge (hard delete) soft-deleted entities older than threshold.
        /// Uses bulk delete for performance.
        /// </summary>
        /// <inheritdoc />
        public async Task<int> PurgeSoftDeletedAsync<TEntity>(DateTime threshold, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            return await _dbContext.Set<TEntity>()
                .Where(e => e.IsDeleted && e.DeletedOnUtc < threshold)
                .ExecuteDeleteAsync(cancellationToken);
        }

        /// <summary>
        /// Deletes (soft) and returns the entity for audit/logging.
        /// </summary>
        /// <inheritdoc />
        public async Task<TEntity?> DeleteAndReturnAsync<TEntity>(object id, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            var entity = await _dbContext.Set<TEntity>().FindAsync(new[] { id }, cancellationToken);
            if (entity == null || entity.IsDeleted) return null;

            MarkAsDeleted(entity);
            MarkSoftDeleteProperties(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return entity;
        }

        #endregion Delete by Condition & Purge
    }
}
