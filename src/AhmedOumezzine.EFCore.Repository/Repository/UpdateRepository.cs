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
    public sealed partial class Repository<TDbContext> : IRepository
        where TDbContext : DbContext
    {


        #region Update (Sync)

        public void Update<TEntity>(TEntity entity) where TEntity : BaseEntity
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            var entry = _dbContext.Entry(entity);

            if (entry.State == EntityState.Detached)
            {
                if (entity.Id == Guid.Empty)
                    throw new InvalidOperationException("Cannot update an entity with an empty Guid Id.");

                SetLastModifiedOnUtc(entity);
                entry.State = EntityState.Modified;
            }
            else
            {
                // Si déjà attachée, EF gère automatiquement LastModifiedOnUtc via SaveChanges
                // Mais on peut forcer si besoin
                SetLastModifiedOnUtc(entity);
                entry.State = EntityState.Modified;
            }
        }

        public void Update<TEntity>(IEnumerable<TEntity> entities) where TEntity : BaseEntity
        {
            if (entities == null) throw new ArgumentNullException(nameof(entities));

            foreach (var entity in entities)
                Update(entity);
        }

        #endregion

        #region Update (Async)

        public async Task<int> UpdateAsync<TEntity>(TEntity entity, CancellationToken ct = default)
            where TEntity : BaseEntity
        {
            Update(entity);
            return await _dbContext.SaveChangesAsync(ct);
        }

        public async Task<int> UpdateAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken ct = default)
            where TEntity : BaseEntity
        {
            Update(entities);
            return await _dbContext.SaveChangesAsync(ct);
        }

        #endregion

        #region Update Only (Partial Update)

        public async Task<int> UpdateOnlyAsync<TEntity>(
            TEntity entity,
            string[] properties,
            CancellationToken ct = default)
            where TEntity : BaseEntity
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (properties == null) throw new ArgumentNullException(nameof(properties));

            if (entity.Id == Guid.Empty)
                throw new InvalidOperationException("Entity must have a valid Id for partial update.");

            var entry = _dbContext.Entry(entity);
            if (entry.State == EntityState.Detached)
            {
                _dbContext.Set<TEntity>().Attach(entity);
                entry.State = EntityState.Unchanged;
            }

            foreach (var propName in properties)
            {
                // Skip primary key and navigation properties
                if (propName == nameof(BaseEntity.Id)) continue;

                var prop = entry.Property(propName);
                if (prop != null && !prop.Metadata.IsPrimaryKey())
                {
                    prop.IsModified = true;
                }
            }

            SetLastModifiedOnUtc(entity);
            return await _dbContext.SaveChangesAsync(ct);
        }

        #endregion

        #region Conditional Update

        public async Task<bool> UpdateIfExistsAsync<TEntity>(
            TEntity entity,
            CancellationToken ct = default)
            where TEntity : BaseEntity
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (entity.Id == Guid.Empty) return false;

            var exists = await _dbContext.Set<TEntity>()
                .AnyAsync(e => e.Id == entity.Id && !e.IsDeleted, ct);

            if (!exists) return false;

            Update(entity);
            await _dbContext.SaveChangesAsync(ct);
            return true;
        }

        #endregion

        #region Safe Update

        public async Task<bool> TryUpdateAsync<TEntity>(
            TEntity entity,
            CancellationToken ct = default)
            where TEntity : BaseEntity
        {
            try
            {
                await UpdateAsync(entity, ct);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Bulk Update (EF Core 7+)

        /// <summary>
        /// Updates entities matching the predicate in a single database roundtrip.
        /// Automatically sets LastModifiedOnUtc.
        /// </summary>
        public async Task<int> UpdateFromQueryAsync<TEntity>(
            Expression<Func<TEntity, bool>> predicate,
            Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> updateAction)
            where TEntity : BaseEntity
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            if (updateAction == null) throw new ArgumentNullException(nameof(updateAction));

            // Créer une nouvelle expression qui applique updateAction PUIS LastModifiedOnUtc
            var param = Expression.Parameter(typeof(SetPropertyCalls<TEntity>), "s");

            // Appel à updateAction(s)
            var call1 = Expression.Invoke(updateAction, param);

            // Appel à SetProperty pour LastModifiedOnUtc
            var setLastModified = typeof(SetPropertyCalls<TEntity>)
                .GetMethod(nameof(SetPropertyCalls<TEntity>.SetProperty))!
                .MakeGenericMethod(typeof(DateTime));

            var propertyExpr = Expression.Lambda<Func<TEntity, DateTime>>(
                Expression.Property(Expression.Parameter(typeof(TEntity), "x"), nameof(BaseEntity.LastModifiedOnUtc)),
                Expression.Parameter(typeof(TEntity), "x"));

            var valueExpr = Expression.Constant(DateTime.UtcNow, typeof(DateTime));
            var call2 = Expression.Call(call1, setLastModified, propertyExpr, valueExpr);

            var finalExpression = Expression.Lambda<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>>(call2, param);

            return await _dbContext.Set<TEntity>()
                .Where(predicate)
                .Where(e => !e.IsDeleted)
                .ExecuteUpdateAsync(finalExpression);
        }
        #endregion
    }
}