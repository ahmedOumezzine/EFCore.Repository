using AhmedOumezzine.EFCore.Repository.Entities;
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
        /// Returns primary key values.
        /// </summary>
        public async Task<object[]> InsertAsync<TEntity>(
            TEntity entity,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            PrepareEntityForInsert(entity);

            await _dbContext.Set<TEntity>().AddAsync(entity, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

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
            if (entities == null) throw new ArgumentNullException(nameof(entities));

            foreach (var entity in entities)
                PrepareEntityForInsert(entity);

            await _dbContext.Set<TEntity>().AddRangeAsync(entities, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
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

        #endregion Basic Inserts

        #region Advanced Inserts

        /// <summary>
        /// Inserts entities in chunks (useful for very large lists).
        /// Each chunk triggers a SaveChanges call.
        /// </summary>
        public async Task InsertManyAsync<TEntity>(
            IEnumerable<TEntity> entities,
            int batchSize = 500,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            if (entities == null) throw new ArgumentNullException(nameof(entities));

            foreach (var batch in entities.Chunk(batchSize))
            {
                foreach (var entity in batch)
                    PrepareEntityForInsert(entity);

                await _dbContext.Set<TEntity>().AddRangeAsync(batch, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        /// <summary>
        /// Inserts with audit logging.
        /// Returns primary key values.
        /// </summary>
        public async Task<object[]> InsertWithAuditAsync<TEntity>(
            TEntity entity,
            string userName,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            PrepareEntityForInsert(entity);

            await _dbContext.Set<TEntity>().AddAsync(entity, cancellationToken);

            // Only log if AuditLog is part of the model
            if (_dbContext.Model.FindEntityType(typeof(AuditLog)) != null)
            {
                var auditLog = new AuditLog
                {
                    Action = "INSERT",
                    EntityName = typeof(TEntity).Name,
                    EntityId = entity.Id.ToString(),
                    UserName = userName,
                    CreatedOnUtc = DateTime.UtcNow
                };
                await _dbContext.Set<AuditLog>().AddAsync(auditLog, cancellationToken);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            var entry = _dbContext.Entry(entity);
            var primaryKey = entry.Metadata.FindPrimaryKey();
            return primaryKey.Properties
                .Select(p => entry.Property(p.Name).CurrentValue)
                .ToArray();
        }

        /// <summary>
        /// Inserts entity only if it does not already exist (based on predicate).
        /// </summary>
        public async Task<bool> InsertIfNotExistsAsync<TEntity>(
            Expression<Func<TEntity, bool>> predicate,
            TEntity entity,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            var exists = await _dbContext.Set<TEntity>().AnyAsync(predicate, cancellationToken);
            if (exists) return false;

            PrepareEntityForInsert(entity);
            await _dbContext.Set<TEntity>().AddAsync(entity, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }

        /// <summary>
        /// Attempts to insert entity and returns false if it fails due to a DbUpdateException (e.g., duplicate key).
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
        /// Insert or update (Upsert) based on a predicate.
        /// ⚠️ Not atomic — use with unique constraints and error handling in production.
        /// </summary>
        public async Task UpsertAsync<TEntity>(
            Expression<Func<TEntity, bool>> predicate,
            TEntity entity,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            var exists = await _dbContext.Set<TEntity>().AnyAsync(predicate, cancellationToken);

            if (exists)
            {
                // Mettre à jour LastModifiedOnUtc automatiquement
                entity.LastModifiedOnUtc = DateTime.UtcNow;
                _dbContext.Set<TEntity>().Update(entity);
            }
            else
            {
                PrepareEntityForInsert(entity);
                await _dbContext.Set<TEntity>().AddAsync(entity, cancellationToken);
            }

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
            if (entities == null) throw new ArgumentNullException(nameof(entities));

            using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                foreach (var entity in entities)
                    PrepareEntityForInsert(entity);

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

        #endregion Advanced Inserts
    }
}