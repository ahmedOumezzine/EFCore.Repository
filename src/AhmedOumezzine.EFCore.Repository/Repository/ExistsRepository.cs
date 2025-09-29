using AhmedOumezzine.EFCore.Repository.Entities;
using AhmedOumezzine.EFCore.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AhmedOumezzine.EFCore.Repository.Repository
{
    /// <summary>
    /// Partial implementation of the generic repository for querying existence of entities.
    /// Provides methods to check if entities exist with or without conditions, or by primary key.
    /// </summary>
    /// <typeparam name="TDbContext">The type of the database context.</typeparam>
    public sealed partial class Repository<TDbContext> : IRepository
        where TDbContext : DbContext
    {
        #region Exists

        /// <summary>
        /// Checks if any non-deleted entity of the specified type exists.
        /// </summary>
        public async Task<bool> ExistsAsync<TEntity>(
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            return await _dbContext.Set<TEntity>()
                .AnyAsync(e => !e.IsDeleted, cancellationToken);
        }

        /// <summary>
        /// Checks if any non-deleted entity matching the condition exists.
        /// </summary>
        public async Task<bool> ExistsAsync<TEntity>(
            Expression<Func<TEntity, bool>> condition,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));

            return await _dbContext.Set<TEntity>()
                .Where(e => !e.IsDeleted)
                .AnyAsync(condition, cancellationToken);
        }

        /// <summary>
        /// Checks if a non-deleted entity with the given primary key exists.
        /// Works with any key type (Guid, int, string, etc.).
        /// </summary>
        public async Task<bool> ExistsByIdAsync<TEntity>(
            object id,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            if (id == null) return false;

            // Optimisation : si l'ID est un Guid, on peut faire une requête directe
            if (id is Guid guidId)
            {
                return await _dbContext.Set<TEntity>()
                    .AnyAsync(e => e.Id == guidId && !e.IsDeleted, cancellationToken);
            }

            // Sinon, fallback générique (utile si tu changes un jour de PK)
            var entityType = _dbContext.Model.FindEntityType(typeof(TEntity));
            var primaryKey = entityType?.FindPrimaryKey()
                ?? throw new ArgumentException($"Entity {typeof(TEntity).Name} has no primary key.");

            var pkProperty = primaryKey.Properties[0];
            var parameter = Expression.Parameter(typeof(TEntity), "e");
            var idProperty = Expression.Property(parameter, pkProperty.Name);
            var isDeletedProperty = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));

            var idEquals = Expression.Equal(idProperty, Expression.Constant(id));
            var notDeleted = Expression.Equal(isDeletedProperty, Expression.Constant(false));
            var body = Expression.AndAlso(idEquals, notDeleted);
            var lambda = Expression.Lambda<Func<TEntity, bool>>(body, parameter);

            return await _dbContext.Set<TEntity>().AnyAsync(lambda, cancellationToken);
        }

        #endregion Exists

        #region Count

        /// <summary>
        /// Counts non-deleted entities of the specified type.
        /// </summary>
        public async Task<int> CountAsync<TEntity>(CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            return await _dbContext.Set<TEntity>()
                .CountAsync(e => !e.IsDeleted, cancellationToken);
        }

        /// <summary>
        /// Counts non-deleted entities matching the condition.
        /// </summary>
        public async Task<int> CountAsync<TEntity>(
            Expression<Func<TEntity, bool>> condition,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));

            return await _dbContext.Set<TEntity>()
                .Where(e => !e.IsDeleted)
                .CountAsync(condition, cancellationToken);
        }

        #endregion Count

        #region (Optional) Composite Key Exists – Only if really needed

        /// <summary>
        /// ⚠️ Use only if you have composite keys.
        /// Checks existence by composite key without loading the entity.
        /// </summary>
        public async Task<bool> ExistsByCompositeKeyAsync<TEntity>(
            object[] keyValues,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            if (keyValues == null || keyValues.Length == 0)
                return false;

            var entityType = _dbContext.Model.FindEntityType(typeof(TEntity));
            var primaryKey = entityType?.FindPrimaryKey()
                ?? throw new ArgumentException($"Entity {typeof(TEntity).Name} has no primary key.");

            if (primaryKey.Properties.Count != keyValues.Length)
                throw new ArgumentException("Key values count does not match primary key properties.");

            var parameter = Expression.Parameter(typeof(TEntity), "e");
            var notDeleted = Expression.Equal(
                Expression.Property(parameter, nameof(BaseEntity.IsDeleted)),
                Expression.Constant(false));

            var conditions = new List<Expression> { notDeleted };

            for (int i = 0; i < primaryKey.Properties.Count; i++)
            {
                var prop = Expression.Property(parameter, primaryKey.Properties[i].Name);
                var value = Expression.Constant(keyValues[i]);
                conditions.Add(Expression.Equal(prop, value));
            }

            var body = conditions.Aggregate(Expression.AndAlso);
            var lambda = Expression.Lambda<Func<TEntity, bool>>(body, parameter);

            return await _dbContext.Set<TEntity>().AnyAsync(lambda, cancellationToken);
        }

        #endregion (Optional) Composite Key Exists – Only if really needed
    }
}