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

        /// <inheritdoc />
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

        /// <inheritdoc />
        public void Update<TEntity>(IEnumerable<TEntity> entities) where TEntity : BaseEntity
        {
            if (entities == null) throw new ArgumentNullException(nameof(entities));

            foreach (var entity in entities)
                Update(entity);
        }

        #endregion

        #region Update (Async)

        /// <inheritdoc />
        public async Task<int> UpdateAsync<TEntity>(TEntity entity, CancellationToken ct = default)
            where TEntity : BaseEntity
        {
            Update(entity);
            return await _dbContext.SaveChangesAsync(ct);
        }

        /// <inheritdoc />
        public async Task<int> UpdateAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken ct = default)
            where TEntity : BaseEntity
        {
            Update(entities);
            return await _dbContext.SaveChangesAsync(ct);
        }

        #endregion

        #region Update Only (Partial Update)

        /// <inheritdoc />
        public async Task<int> UpdateOnlyAsync<TEntity>(
            TEntity entity,
            string[] properties,
            CancellationToken ct = default)
            where TEntity : BaseEntity
        {
            ArgumentNullException.ThrowIfNull(entity);
            ArgumentNullException.ThrowIfNull(properties);
            var entityType = _dbContext.Model.FindEntityType(typeof(TEntity))
                ?? throw new InvalidOperationException($"Entity type {typeof(TEntity).Name} is not part of the model.");
            foreach (var name in properties)
            {
                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentException("Property names cannot be empty.", nameof(properties));
                var property = entityType.FindProperty(name);
                if (property == null)
                    throw new ArgumentException($"Property '{name}' was not found on {typeof(TEntity).Name}.", nameof(properties));
                if (property.IsPrimaryKey())
                    throw new ArgumentException($"Property '{name}' is a primary key and cannot be updated.", nameof(properties));
                if (entityType.FindNavigation(name) != null || entityType.FindSkipNavigation(name) != null)
                    throw new ArgumentException($"Property '{name}' is a navigation and cannot be updated.", nameof(properties));
            }

            if (entity.Id == Guid.Empty)
                throw new InvalidOperationException("Entity must have a valid Id for partial update.");

            var entry = _dbContext.Entry(entity);
            var local = _dbContext.Set<TEntity>().Local.FirstOrDefault(e => e.Id == entity.Id);
            if (local != null && !ReferenceEquals(local, entity))
            {
                foreach (var propName in properties)
                {
                    var metadata = entityType.FindProperty(propName);
                        if (metadata?.PropertyInfo is { } propertyInfo)
                            propertyInfo.SetValue(local, propertyInfo.GetValue(entity));
                }
                entity = local;
                entry = _dbContext.Entry(entity);
            }
            else if (entry.State == EntityState.Detached)
            {
                var tracked = _dbContext.Set<TEntity>().Local.FirstOrDefault(e => e.Id == entity.Id);
                if (tracked != null)
                {
                    foreach (var propName in properties)
                    {
                        var metadata = entityType.FindProperty(propName);
                        if (metadata != null && !metadata.IsPrimaryKey() && metadata.PropertyInfo is { } propertyInfo)
                            propertyInfo.SetValue(tracked, propertyInfo.GetValue(entity));
                    }
                    entity = tracked;
                    entry = _dbContext.Entry(entity);
                }
                else
                {
                    _dbContext.Set<TEntity>().Attach(entity);
                }
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

        /// <inheritdoc />
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

        /// <inheritdoc />
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
            catch (OperationCanceledException)
            {
                throw;
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
        /// <inheritdoc />
        public async Task<int> UpdateFromQueryAsync<TEntity>(
            Expression<Func<TEntity, bool>> predicate,
            Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> updateAction)
            where TEntity : BaseEntity
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            if (updateAction == null) throw new ArgumentNullException(nameof(updateAction));

            var query = _dbContext.Set<TEntity>()
                .Where(predicate)
                .Where(e => !e.IsDeleted);

            var affected = await query.ExecuteUpdateAsync(updateAction);
            if (affected == 0)
                return 0;

            await query.ExecuteUpdateAsync(setters => setters
                .SetProperty(e => e.LastModifiedOnUtc, DateTime.UtcNow));
            return affected;
        }
        #endregion
    }
}
