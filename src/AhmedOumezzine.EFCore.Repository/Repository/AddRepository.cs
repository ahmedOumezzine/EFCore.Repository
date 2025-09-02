using AhmedOumezzine.EFCore.Repository.Entities;
using AhmedOumezzine.EFCore.Repository.Entities.AhmedOumezzine.EFCore.Repository.Entities;
using AhmedOumezzine.EFCore.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AhmedOumezzine.EFCore.Repository.Repository
{
    /// <summary>
    /// Generic repository implementation focused on insert operations (with immediate persistence).
    /// </summary>
    /// <typeparam name="TDbContext">The database context type.</typeparam>
    public sealed partial class Repository<TDbContext> : IRepository
        where TDbContext : DbContext
    {
       
        #region Basic Inserts

        /// <summary>
        /// Inserts a single entity and saves changes immediately.
        /// </summary>
        public async Task<object[]> InsertAsync<TEntity>(
            TEntity entity,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            CheckEntityIsNull(entity);
            SetCreatedOnUtc(entity);

            await _dbContext.Set<TEntity>().AddAsync(entity, cancellationToken).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            var entry = _dbContext.Entry(entity);
            var primaryKey = entry.Metadata.FindPrimaryKey();
            return primaryKey.Properties
                .Select(p => entry.Property(p.Name).CurrentValue)
                .ToArray();
        }

        /// <summary>
        /// Inserts a collection of entities and saves changes immediately.
        /// </summary>
        public async Task InsertRangeAsync<TEntity>(
            IEnumerable<TEntity> entities,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            CheckEntitiesIsNull(entities);
            SetCreatedOnUtc(entities);

            await _dbContext.Set<TEntity>().AddRangeAsync(entities, cancellationToken).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Inserts and returns the entity (useful for getting generated keys).
        /// </summary>
        public async Task<TEntity> InsertAndReturnAsync<TEntity>(
            TEntity entity,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            await InsertAsync(entity, cancellationToken);
            return entity;
        }

        #endregion

        #region Advanced Inserts

        /// <summary>
        /// Bulk inserts entities in a single batch.
        /// </summary>
        public async Task BulkInsertAsync<TEntity>(
            IEnumerable<TEntity> entities,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            await InsertRangeAsync(entities, cancellationToken);
        }

        /// <summary>
        /// Inserts entities in chunks (useful for very large lists).
        /// </summary>
        public async Task InsertManyAsync<TEntity>(
            IEnumerable<TEntity> entities,
            int batchSize = 500,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            foreach (var batch in entities.Chunk(batchSize))
            {
                await InsertRangeAsync(batch, cancellationToken);
            }
        }

        /// <summary>
        /// Inserts with audit logging.
        /// Requires an AuditLog entity in DbContext.
        /// </summary>
        public async Task<object[]> InsertWithAuditAsync<TEntity>(
            TEntity entity,
            string userName,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            CheckEntityIsNull(entity);
            SetCreatedOnUtc(entity);

            await _dbContext.Set<TEntity>().AddAsync(entity, cancellationToken);

            var auditLog = new AuditLog
            {
                Action = "INSERT",
                EntityName = typeof(TEntity).Name,
                EntityId = entity.Id.ToString(),
                UserName = userName,
                CreatedOnUtc = DateTime.UtcNow
            };
            await _dbContext.Set<AuditLog>().AddAsync(auditLog, cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);

            var entry = _dbContext.Entry(entity);
            var primaryKey = entry.Metadata.FindPrimaryKey();
            return primaryKey.Properties
                .Select(p => entry.Property(p.Name).CurrentValue)
                .ToArray();
        }

        /// <summary>
        /// Inserts entity only if it does not already exist.
        /// </summary>
        public async Task<bool> InsertIfNotExistsAsync<TEntity>(
            Expression<Func<TEntity, bool>> predicate,
            TEntity entity,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            CheckEntityIsNull(entity);

            var exists = await _dbContext.Set<TEntity>().AnyAsync(predicate, cancellationToken);
            if (exists) return false;

            await _dbContext.Set<TEntity>().AddAsync(entity, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }

        /// <summary>
        /// Attempts to insert entity and returns false if it fails due to a DbUpdateException.
        /// </summary>
        public async Task<bool> TryInsertAsync<TEntity>(
            TEntity entity,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            try
            {
                await InsertAsync(entity, cancellationToken);
                return true;
            }
            catch (DbUpdateException)
            {
                return false;
            }
        }

        /// <summary>
        /// Insert or update (Upsert).
        /// </summary>
        public async Task UpsertAsync<TEntity>(
            Expression<Func<TEntity, bool>> predicate,
            TEntity entity,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            var exists = await _dbContext.Set<TEntity>().AnyAsync(predicate, cancellationToken);

            if (exists)
                _dbContext.Set<TEntity>().Update(entity);
            else
                await _dbContext.Set<TEntity>().AddAsync(entity, cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Inserts a graph of entities including related data.
        /// </summary>
        public async Task InsertGraphAsync<TEntity>(
            TEntity entity,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            CheckEntityIsNull(entity);
            SetCreatedOnUtc(entity);

            await _dbContext.AddAsync(entity, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Inserts entities in a transaction (all-or-nothing).
        /// </summary>
        public async Task InsertWithTransactionAsync<TEntity>(
            IEnumerable<TEntity> entities,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                await _dbContext.Set<TEntity>().AddRangeAsync(entities, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        #endregion

        
    }
}
