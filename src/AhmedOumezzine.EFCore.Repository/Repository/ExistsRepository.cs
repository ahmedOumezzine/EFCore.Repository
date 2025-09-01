using AhmedOumezzine.EFCore.Repository.Entities;
using AhmedOumezzine.EFCore.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

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

        #region ExistsByIdAsync (Correct Signature)

        /// <summary>
        /// Checks if an entity of the specified type with the given Guid ID exists and is not soft-deleted.
        /// </summary>
        public async Task<bool> ExistsByIdAsync<TEntity>(
            Guid? id,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            if (id == null)
                return false;

            return await _dbContext.Set<TEntity>()
                .AnyAsync(e => e.Id == id && !e.IsDeleted, cancellationToken);
        }

        #endregion

        #region Other Exists Methods

        /// <summary>
        /// Checks if any entity of the specified type exists.
        /// </summary>
        public async Task<bool> ExistsAsync<TEntity>(
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            return await _dbContext.Set<TEntity>()
                .AnyAsync(e => !e.IsDeleted, cancellationToken);
        }

        /// <summary>
        /// Checks if any entity matching the condition exists.
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

        #endregion
         

        #region Exists By Primary Key

        /// <summary>
        /// Checks if an entity of the specified type with the given primary key value exists and is not soft-deleted.
        /// Supports any primary key type (int, Guid, string, etc.).
        /// </summary>
        /// <typeparam name="TEntity">The entity type, must inherit from <see cref="BaseEntity"/>.</typeparam>
        /// <param name="keyValue">The primary key value to search for.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>True if the entity exists and is not deleted; otherwise, false.</returns>
        /// <exception cref="ArgumentException">Thrown when the entity has no primary key defined.</exception>
        public async Task<bool> ExistsByIdAsync<TEntity>(
            object keyValue,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            if (keyValue == null)
                return false;

            var entityType = _dbContext.Model.FindEntityType(typeof(TEntity));
            var primaryKey = entityType?.FindPrimaryKey();

            if (primaryKey == null)
            {
                throw new ArgumentException($"Entity {typeof(TEntity).Name} does not have a primary key defined.");
            }

            // Construire l'expression : e => e.PK == keyValue && e.IsDeleted == false
            var parameter = Expression.Parameter(typeof(TEntity), "e");
            var property = Expression.Property(parameter, primaryKey.Properties[0].Name);
            var equals = Expression.Equal(property, Expression.Constant(keyValue));
            var isNotDeleted = Expression.Equal(
                Expression.Property(parameter, nameof(BaseEntity.IsDeleted)),
                Expression.Constant(false));
            var and = Expression.AndAlso(equals, isNotDeleted);
            var lambda = Expression.Lambda<Func<TEntity, bool>>(and, parameter);

            return await _dbContext.Set<TEntity>().AnyAsync(lambda, cancellationToken).ConfigureAwait(false);
        }

        #endregion

        #region Exists By Composite Key (Optional)

        /// <summary>
        /// Checks if an entity with the specified composite key values exists.
        /// Example: ExistsByCompositeKeyAsync<User>(new object[] { "tenant1", 123 }).
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="keyValues">An array of key values in the order of the primary key properties.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>True if the entity exists; otherwise, false.</returns>
        public async Task<bool> ExistsByCompositeKeyAsync<TEntity>(
            object[] keyValues,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            if (keyValues == null || keyValues.Length == 0)
                return false;

            var entity = _dbContext.Set<TEntity>().Find(keyValues);
            return entity != null && !entity.IsDeleted;
        }

        #endregion

        #region Count

        /// <summary>
        /// Counts the number of entities of the specified type that are not soft-deleted.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The number of non-deleted entities.</returns>
        public async Task<int> CountAsync<TEntity>(CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            return await _dbContext.Set<TEntity>()
                .CountAsync(e => !e.IsDeleted, cancellationToken);
        }

        /// <summary>
        /// Counts the number of entities matching the specified condition and not soft-deleted.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="condition">The condition to match.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The number of matching entities.</returns>
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

        #endregion
    }
}