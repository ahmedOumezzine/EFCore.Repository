using AhmedOumezzine.EFCore.Repository.Entities;
using AhmedOumezzine.EFCore.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace AhmedOumezzine.EFCore.Repository.Repository
{
    /// <summary>
    /// Partial implementation of the generic repository for retrieving entities by primary key.
    /// Supports single and batch retrieval, projections, property selection, and safe access.
    /// Automatically excludes soft-deleted entities.
    /// </summary>
    /// <typeparam name="TDbContext">The type of the database context.</typeparam>
    public sealed partial class Repository<TDbContext> : IRepository
        where TDbContext : DbContext
    {

        #region GetByIdAsync - Missing Overload

        /// <summary>
        /// Retrieves an entity by its primary key with optional includes.
        /// </summary>
        public async Task<TEntity> GetByIdAsync<TEntity>(
            Guid? id,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> includes,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            var query = _dbContext.Set<TEntity>().Where(e => e.Id == id && !e.IsDeleted);

            if (includes != null)
            {
                query = includes(query);
            }

            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        #endregion

        #region Autres surcharges de GetByIdAsync (existantes)

        /// <summary>
        /// Retrieves an entity by its primary key.
        /// </summary>
        public async Task<TEntity> GetByIdAsync<TEntity>(
            Guid? id,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            var query = _dbContext.Set<TEntity>().Where(e => e.Id == id && !e.IsDeleted);

          

            return await query.FirstOrDefaultAsync(cancellationToken);
        }
        /// <summary>
        /// Retrieves an entity by its primary key with optional tracking.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="id">The primary key value.</param>
        /// <param name="asNoTracking">If true, the entity will not be tracked.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The entity if found; otherwise, null.</returns>
        public async Task<TEntity> GetByIdAsync<TEntity>(
            Guid? id,
            bool asNoTracking,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            var query = _dbContext.Set<TEntity>().Where(e => e.Id == id && !e.IsDeleted);

            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            return await query.FirstOrDefaultAsync(cancellationToken);
        }
        /// <summary>
        /// Retrieves an entity by its primary key with includes and optional tracking.
        /// </summary>
        public async Task<TEntity> GetByIdAsync<TEntity>(
            Guid? id,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> includes,
            bool asNoTracking,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            var query = _dbContext.Set<TEntity>().Where(e => e.Id == id && !e.IsDeleted);

            if (includes != null)
            {
                query = includes(query);
            }

            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Retrieves a projected entity by its primary key.
        /// </summary>
        public async Task<TProjectedType> GetByIdAsync<TEntity, TProjectedType>(
            Guid? id,
            Expression<Func<TEntity, TProjectedType>> selectExpression,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
            where TProjectedType : class
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (selectExpression == null) throw new ArgumentNullException(nameof(selectExpression));

            return await _dbContext.Set<TEntity>()
                .Where(e => e.Id == id && !e.IsDeleted)
                .Select(selectExpression)
                .FirstOrDefaultAsync(cancellationToken);
        }

        #endregion
        #region GetByIdAsync (Main Overload)

        /// <summary>
        /// Asynchronously retrieves an entity by its primary key value.
        /// </summary>
        /// <typeparam name="TEntity">The entity type, must inherit from <see cref="BaseEntity"/>.</typeparam>
        /// <param name="id">The primary key value. Cannot be null.</param>
        /// <param name="includes">Optional: Specifies related data to include in the query.</param>
        /// <param name="asNoTracking">If true, the entity will not be tracked by the context.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The entity if found and not soft-deleted; otherwise, null.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="id"/> is null.</exception>
        public async Task<TEntity> GetByIdAsync<TEntity>(
            object id,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> includes = null,
            bool asNoTracking = false,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            var query = _dbContext.Set<TEntity>() as IQueryable<TEntity>;

            // Apply includes
            if (includes != null)
            {
                query = includes(query);
            }

            // Apply tracking
            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            // Build condition: e => EF.Property<object>(e, "Id") == id && !e.IsDeleted
            var parameter = Expression.Parameter(typeof(TEntity), "e");
            var property = Expression.Call(
                typeof(EF), nameof(EF.Property), new[] { id.GetType() },
                parameter, Expression.Constant("Id"));
            var equals = Expression.Equal(property, Expression.Constant(id));
            var isNotDeleted = Expression.Equal(
                Expression.Property(parameter, nameof(BaseEntity.IsDeleted)),
                Expression.Constant(false));
            var and = Expression.AndAlso(equals, isNotDeleted);
            var lambda = Expression.Lambda<Func<TEntity, bool>>(and, parameter);

            return await query.FirstOrDefaultAsync(lambda, cancellationToken);
        }

        #endregion

        #region GetByIdAsync (Overloads)

        /// <summary>
        /// Retrieves an entity by its primary key (with tracking enabled).
        /// </summary>
        public Task<TEntity> GetByIdAsync<TEntity>(object id, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            return GetByIdAsync<TEntity>(id, includes: null, asNoTracking: false, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Retrieves an entity by its primary key with includes (tracking enabled).
        /// </summary>
        public Task<TEntity> GetByIdAsync<TEntity>(
            object id,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> includes,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            return GetByIdAsync<TEntity>(id, includes, asNoTracking: false, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Retrieves an entity by its primary key without tracking.
        /// </summary>
        public Task<TEntity> GetByIdAsync<TEntity>(object id, bool asNoTracking, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            return GetByIdAsync<TEntity>(id, includes: null, asNoTracking: asNoTracking, cancellationToken: cancellationToken);
        }

        #endregion

        #region GetByIdsAsync (Batch Load)

        /// <summary>
        /// Retrieves multiple entities by their primary key values.
        /// Automatically excludes soft-deleted entities.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="ids">The collection of primary key values.</param>
        /// <param name="asNoTracking">If true, entities won't be tracked.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>A list of entities matching the IDs and not soft-deleted.</returns>
        public async Task<List<TEntity>> GetByIdsAsync<TEntity>(
            IEnumerable<object> ids,
            bool asNoTracking = false,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            if (ids == null || !ids.Any())
                return new List<TEntity>();

            var query = _dbContext.Set<TEntity>() as IQueryable<TEntity>;

            // Build: e => ids.Contains(EF.Property<object>(e, "Id")) && !e.IsDeleted
            var parameter = Expression.Parameter(typeof(TEntity), "e");
            var property = Expression.Call(
                typeof(EF), nameof(EF.Property), new[] { typeof(object) },
                parameter, Expression.Constant("Id"));
            var contains = Expression.Call(
                typeof(Enumerable), "Contains", new[] { typeof(object) },
                Expression.Constant(ids.ToList()),
                property);
            var notDeleted = Expression.Equal(
                Expression.Property(parameter, nameof(BaseEntity.IsDeleted)),
                Expression.Constant(false));
            var and = Expression.AndAlso(contains, notDeleted);
            var lambda = Expression.Lambda<Func<TEntity, bool>>(and, parameter);

            query = query.Where(lambda);

            if (asNoTracking)
                query = query.AsNoTracking();

            return await query.ToListAsync(cancellationToken);
        }

        #endregion

        #region GetByIdOrDefaultAsync & FindByIdAsync (Semantic Aliases)

        /// <summary>
        /// Retrieves an entity by ID or returns null if not found.
        /// </summary>
        public Task<TEntity> GetByIdOrDefaultAsync<TEntity>(object id, CancellationToken ct = default)
            where TEntity : BaseEntity
        {
            return GetByIdAsync<TEntity>(id, ct);
        }

        /// <summary>
        /// Finds an entity by its primary key value.
        /// </summary>
        public Task<TEntity> FindByIdAsync<TEntity>(object id, CancellationToken ct = default)
            where TEntity : BaseEntity
        {
            return GetByIdAsync<TEntity>(id, ct);
        }

        #endregion

        #region GetOnlyAsync (Projection of Single Property)

        /// <summary>
        /// Retrieves a single property value of an entity by ID.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <typeparam name="TProperty">The property type (e.g., string, DateTime).</typeparam>
        /// <param name="id">The primary key value.</param>
        /// <param name="propertySelector">The property to retrieve (e.g., u => u.Name).</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The property value, or default if not found.</returns>
        public async Task<TProperty> GetOnlyAsync<TEntity, TProperty>(
            object id,
            Expression<Func<TEntity, TProperty>> propertySelector,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            if (id == null) return default;

            var parameter = Expression.Parameter(typeof(TEntity), "e");
            var property = Expression.Call(
                typeof(EF), nameof(EF.Property), new[] { id.GetType() },
                parameter, Expression.Constant("Id"));
            var equals = Expression.Equal(property, Expression.Constant(id));
            var notDeleted = Expression.Equal(
                Expression.Property(parameter, nameof(BaseEntity.IsDeleted)),
                Expression.Constant(false));
            var and = Expression.AndAlso(equals, notDeleted);
            var lambda = Expression.Lambda<Func<TEntity, bool>>(and, parameter);

            return await _dbContext.Set<TEntity>()
                .Where(lambda)
                .Select(propertySelector)
                .FirstOrDefaultAsync(cancellationToken);
        }

        #endregion

        #region TryGetByIdAsync (Safe Access)

        /// <summary>
        /// Attempts to retrieve an entity by ID without throwing.
        /// </summary>
        /// <returns>True and the entity if found; otherwise, False and null.</returns>
        public async Task<(bool Success, TEntity Entity)> TryGetByIdAsync<TEntity>(
            object id,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            try
            {
                var entity = await GetByIdAsync<TEntity>(id, cancellationToken);
                return (entity != null, entity);
            }
            catch
            {
                return (false, null);
            }
        }

        #endregion

        #region Private Helpers

        /// <summary>
        /// Builds a LINQ expression for e.Id == id && !e.IsDeleted.
        /// </summary>
        private Expression<Func<T, bool>> BuildIdCondition<T>(object id) where T : BaseEntity
        {
            var param = Expression.Parameter(typeof(T), "e");
            var property = Expression.Call(
                typeof(EF), nameof(EF.Property), new[] { id.GetType() },
                param, Expression.Constant("Id"));
            var equals = Expression.Equal(property, Expression.Constant(id));
            var notDeleted = Expression.Equal(
                Expression.Property(param, nameof(BaseEntity.IsDeleted)),
                Expression.Constant(false));
            return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(equals, notDeleted), param);
        }

        #endregion
    }
}