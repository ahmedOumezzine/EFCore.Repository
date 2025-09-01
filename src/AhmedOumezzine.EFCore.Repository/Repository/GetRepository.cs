using AhmedOumezzine.EFCore.Repository.Entities;
using AhmedOumezzine.EFCore.Repository.Interface;
using AhmedOumezzine.EFCore.Repository.Specification;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace AhmedOumezzine.EFCore.Repository.Repository
{
    /// <summary>
    /// Partial implementation of the generic repository for retrieving a single entity.
    /// Supports filtering, projection, includes, specifications, and safe operations.
    /// Automatically excludes soft-deleted entities.
    /// </summary>
    /// <typeparam name="TDbContext">The type of the database context.</typeparam>
    public sealed partial class Repository<TDbContext> : IRepository
        where TDbContext : DbContext
    {
       
        #region GetAsync - By Condition

        /// <summary>
        /// Retrieves the first entity matching the condition, or null if not found.
        /// Automatically excludes soft-deleted entities.
        /// </summary>
        public Task<TEntity> GetAsync<TEntity>(
            Expression<Func<TEntity, bool>> condition,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            return GetAsync(condition, includes: null, asNoTracking: false, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Retrieves the first entity matching the condition with optional tracking.
        /// </summary>
        public Task<TEntity> GetAsync<TEntity>(
            Expression<Func<TEntity, bool>> condition,
            bool asNoTracking,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            return GetAsync(condition, includes: null, asNoTracking: asNoTracking, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Retrieves the first entity matching the condition with includes.
        /// </summary>
        public Task<TEntity> GetAsync<TEntity>(
            Expression<Func<TEntity, bool>> condition,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> includes,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            return GetAsync(condition, includes, asNoTracking: false, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Retrieves the first entity matching the condition with includes and optional tracking.
        /// </summary>
        public async Task<TEntity> GetAsync<TEntity>(
            Expression<Func<TEntity, bool>> condition,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> includes,
            bool asNoTracking,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            var query = _dbContext.Set<TEntity>().Where(e => !e.IsDeleted);

            if (condition != null)
                query = query.Where(condition);

            if (includes != null)
                query = includes(query);

            if (asNoTracking)
                query = query.AsNoTracking();

            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        #endregion

        #region GetAsync - By Specification

        /// <summary>
        /// Retrieves the first entity matching the specification.
        /// </summary>
        public Task<TEntity> GetAsync<TEntity>(
            Specification<TEntity> specification,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            return GetAsync(specification, asNoTracking: false, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Retrieves the first entity matching the specification with optional tracking.
        /// </summary>
        public async Task<TEntity> GetAsync<TEntity>(
            Specification<TEntity> specification,
            bool asNoTracking,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            if (specification == null)
                throw new ArgumentNullException(nameof(specification));

            var query = _dbContext.Set<TEntity>().AsQueryable();
            query = query.GetSpecifiedQuery(specification);
            query = query.Where(e => !e.IsDeleted);

            if (asNoTracking)
                query = query.AsNoTracking();

            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        #endregion

        #region GetAsync - Projection (Select)

        /// <summary>
        /// Retrieves a projected entity (e.g., DTO) based on a condition.
        /// </summary>
        public async Task<TProjectedType> GetAsync<TEntity, TProjectedType>(
            Expression<Func<TEntity, bool>> condition,
            Expression<Func<TEntity, TProjectedType>> selectExpression,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
            where TProjectedType : class
        {
            if (selectExpression == null)
                throw new ArgumentNullException(nameof(selectExpression));

            var query = _dbContext.Set<TEntity>().Where(e => !e.IsDeleted);

            if (condition != null)
                query = query.Where(condition);

            return await query.Select(selectExpression).FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Retrieves a projected entity based on a specification.
        /// </summary>
        public async Task<TProjectedType> GetAsync<TEntity, TProjectedType>(
            Specification<TEntity> specification,
            Expression<Func<TEntity, TProjectedType>> selectExpression,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
            where TProjectedType : class
        {
            if (selectExpression == null)
                throw new ArgumentNullException(nameof(selectExpression));
            if (specification == null)
                throw new ArgumentNullException(nameof(specification));

            var query = _dbContext.Set<TEntity>().AsQueryable();
            query = query.GetSpecifiedQuery(specification);
            query = query.Where(e => !e.IsDeleted);

            return await query.Select(selectExpression).FirstOrDefaultAsync(cancellationToken);
        }

        #endregion

        #region TryGetAsync (Safe Access)

        /// <summary>
        /// Attempts to retrieve an entity without throwing.
        /// </summary>
        /// <returns>True and the entity if found; otherwise, False and null.</returns>
        public async Task<(bool Success, TEntity Entity)> TryGetAsync<TEntity>(
            Expression<Func<TEntity, bool>> condition,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            try
            {
                var entity = await GetAsync(condition, cancellationToken);
                return (entity != null, entity);
            }
            catch
            {
                return (false, null);
            }
        }

        #endregion

        #region GetAsyncOrDefault (Semantic Clarity)

        /// <summary>
        /// Retrieves the first entity matching the condition or returns null.
        /// Alias for GetAsync for semantic clarity.
        /// </summary>
        public Task<TEntity> GetAsyncOrDefault<TEntity>(
            Expression<Func<TEntity, bool>> condition,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            return GetAsync(condition, cancellationToken);
        }

        #endregion

        #region FindAsync (Semantic Alias)

        /// <summary>
        /// Finds the first entity matching the condition (alias for GetAsync).
        /// </summary>
        public Task<TEntity> FindAsync<TEntity>(
            Expression<Func<TEntity, bool>> condition,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            return GetAsync(condition, cancellationToken);
        }

        #endregion

        #region GetFirstOrThrowAsync (Fail Fast)

        /// <summary>
        /// Retrieves the first entity matching the condition or throws if not found.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when no entity is found.</exception>
        public async Task<TEntity> GetFirstOrThrowAsync<TEntity>(
            Expression<Func<TEntity, bool>> condition,
            string errorMessage = null,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            var entity = await GetAsync(condition, cancellationToken);
            if (entity == null)
                throw new InvalidOperationException(errorMessage ?? $"Entity of type {typeof(TEntity).Name} not found for the given condition.");
            return entity;
        }

        #endregion

        #region ExistsAndFetchAsync (Optimized Load)

        /// <summary>
        /// Checks if an entity exists and returns it if so.
        /// Reduces database roundtrips in "check and load" scenarios.
        /// </summary>
        public async Task<(bool Exists, TEntity Entity)> ExistsAndFetchAsync<TEntity>(
            Expression<Func<TEntity, bool>> condition,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            var entity = await GetAsync(condition, cancellationToken);
            return (entity != null, entity);
        }

        #endregion

        #region GetOnlyAsync (Single Property)

        /// <summary>
        /// Retrieves a single property value of an entity (e.g., Name, Email).
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <typeparam name="TProperty">The property type (e.g., string, DateTime).</typeparam>
        /// <param name="condition">The condition to find the entity.</param>
        /// <param name="propertySelector">The property to retrieve (e.g., u => u.Name).</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The property value, or default if not found.</returns>
        public async Task<TProperty> GetOnlyAsync<TEntity, TProperty>(
            Expression<Func<TEntity, bool>> condition,
            Expression<Func<TEntity, TProperty>> propertySelector,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            if (condition == null) throw new ArgumentNullException(nameof(condition));
            if (propertySelector == null) throw new ArgumentNullException(nameof(propertySelector));

            return await _dbContext.Set<TEntity>()
                .Where(e => !e.IsDeleted)
                .Where(condition)
                .Select(propertySelector)
                .FirstOrDefaultAsync(cancellationToken);
        }

        #endregion
    }
}