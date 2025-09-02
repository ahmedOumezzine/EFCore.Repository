using AhmedOumezzine.EFCore.Repository.Entities;
using AhmedOumezzine.EFCore.Repository.Extensions;
using AhmedOumezzine.EFCore.Repository.Interface;
using AhmedOumezzine.EFCore.Repository.Specification;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace AhmedOumezzine.EFCore.Repository.Repository
{
    /// <summary>
    /// Partial implementation of the generic repository for retrieving lists of entities.
    /// Supports filtering, projection, pagination, includes, specifications, and safe operations.
    /// Automatically excludes soft-deleted entities in standard methods.
    /// </summary>
    /// <typeparam name="TDbContext">The type of the database context.</typeparam>
    public sealed partial class Repository<TDbContext> : IRepository
        where TDbContext : DbContext
    {
        #region GetListAsync - Base Overloads (No Filter)

        /// <summary>
        /// Retrieves all non-deleted entities of the specified type.
        /// </summary>
        public Task<List<TEntity>> GetListAsync<TEntity>(CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            return GetListAsync<TEntity>(includes: null, asNoTracking: false, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Retrieves all non-deleted entities with optional tracking control.
        /// </summary>
        public Task<List<TEntity>> GetListAsync<TEntity>(bool asNoTracking, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            return GetListAsync<TEntity>(includes: null, asNoTracking: asNoTracking, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Retrieves all non-deleted entities with optional includes.
        /// </summary>
        public Task<List<TEntity>> GetListAsync<TEntity>(
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> includes,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            return GetListAsync(includes, asNoTracking: false, cancellationToken);
        }

        /// <summary>
        /// Retrieves all non-deleted entities with includes and optional tracking.
        /// </summary>
        public async Task<List<TEntity>> GetListAsync<TEntity>(
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> includes,
            bool asNoTracking,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            var query = _dbContext.Set<TEntity>().Where(e => !e.IsDeleted);

            if (includes != null)
                query = includes(query);

            if (asNoTracking)
                query = query.AsNoTracking();

            return await query.ToListAsync(cancellationToken);
        }

        #endregion GetListAsync - Base Overloads (No Filter)

        #region GetListAsync - With Condition

        /// <summary>
        /// Retrieves entities matching the condition.
        /// </summary>
        public Task<List<TEntity>> GetListAsync<TEntity>(
            Expression<Func<TEntity, bool>> condition,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            return GetListAsync(condition, includes: null, asNoTracking: false, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Retrieves entities matching the condition with optional tracking.
        /// </summary>
        public Task<List<TEntity>> GetListAsync<TEntity>(
            Expression<Func<TEntity, bool>> condition,
            bool asNoTracking,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            return GetListAsync(condition, includes: null, asNoTracking: asNoTracking, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Retrieves entities matching the condition with includes and optional tracking.
        /// </summary>
        public async Task<List<TEntity>> GetListAsync<TEntity>(
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

            return await query.ToListAsync(cancellationToken);
        }

        #endregion GetListAsync - With Condition

        #region GetListAsync - With Specification

        /// <summary>
        /// Retrieves entities matching the specification.
        /// </summary>
        public Task<List<TEntity>> GetListAsync<TEntity>(
            Specification<TEntity> specification,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            return GetListAsync(specification, asNoTracking: false, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Retrieves entities matching the specification with optional tracking.
        /// </summary>
        public async Task<List<TEntity>> GetListAsync<TEntity>(
            Specification<TEntity> specification,
            bool asNoTracking,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            var query = _dbContext.Set<TEntity>().AsQueryable();

            if (specification != null)
                query = query.GetSpecifiedQuery(specification);

            query = query.Where(e => !e.IsDeleted);

            if (asNoTracking)
                query = query.AsNoTracking();

            return await query.ToListAsync(cancellationToken);
        }

        #endregion GetListAsync - With Specification

        #region GetListAsync - Projection (Select)

        /// <summary>
        /// Retrieves a list of projected entities (e.g., DTOs).
        /// </summary>
        public async Task<List<TProjectedType>> GetListAsync<TEntity, TProjectedType>(
            Expression<Func<TEntity, TProjectedType>> selectExpression,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
            where TProjectedType : class
        {
            if (selectExpression == null)
                throw new ArgumentNullException(nameof(selectExpression));

            return await _dbContext.Set<TEntity>()
                .Where(e => !e.IsDeleted)
                .Select(selectExpression)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Retrieves projected entities matching a condition.
        /// </summary>
        public async Task<List<TProjectedType>> GetListAsync<TEntity, TProjectedType>(
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

            return await query.Select(selectExpression).ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Retrieves projected entities matching a specification.
        /// </summary>
        public async Task<List<TProjectedType>> GetListAsync<TEntity, TProjectedType>(
            Specification<TEntity> specification,
            Expression<Func<TEntity, TProjectedType>> selectExpression,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
            where TProjectedType : class
        {
            if (selectExpression == null)
                throw new ArgumentNullException(nameof(selectExpression));

            var query = _dbContext.Set<TEntity>().AsQueryable();

            if (specification != null)
                query = query.GetSpecifiedQuery(specification);

            query = query.Where(e => !e.IsDeleted);

            return await query.Select(selectExpression).ToListAsync(cancellationToken);
        }

        #endregion GetListAsync - Projection (Select)

        #region GetListAsync - Pagination

        /// <summary>
        /// Retrieves a paginated list of entities.
        /// </summary>
        public async Task<PaginatedList<TEntity>> GetListAsync<TEntity>(
            PaginationSpecification<TEntity> specification,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            if (specification == null)
                throw new ArgumentNullException(nameof(specification));

            var query = _dbContext.Set<TEntity>().Where(e => !e.IsDeleted);
            return await query.ToPaginatedListAsync(specification, cancellationToken);
        }

        /// <summary>
        /// Retrieves a paginated list of projected entities.
        /// </summary>
        public async Task<PaginatedList<TProjectedType>> GetListAsync<TEntity, TProjectedType>(
            PaginationSpecification<TEntity> specification,
            Expression<Func<TEntity, TProjectedType>> selectExpression,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
            where TProjectedType : class
        {
            if (specification == null)
                throw new ArgumentNullException(nameof(specification));
            if (selectExpression == null)
                throw new ArgumentNullException(nameof(selectExpression));

            var query = _dbContext.Set<TEntity>().AsQueryable();

            if (specification != null)
                query = query.GetSpecifiedQuery((SpecificationBase<TEntity>)specification);

            query = query.Where(e => !e.IsDeleted);

            return await query.Select(selectExpression)
                .ToPaginatedListAsync(specification.PageIndex, specification.PageSize, cancellationToken);
        }

        #endregion GetListAsync - Pagination

        #region GetActiveListAsync (Semantic Alias)

        /// <summary>
        /// Retrieves a list of non-soft-deleted entities.
        /// Explicit alias for GetListAsync with soft-delete filtering.
        /// </summary>
        public Task<List<TEntity>> GetActiveListAsync<TEntity>(CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            return GetListAsync<TEntity>(cancellationToken);
        }

        #endregion GetActiveListAsync (Semantic Alias)

        #region GetDeletedListAsync (Audit / Restore)

        /// <summary>
        /// Retrieves a list of soft-deleted entities.
        /// Useful for audit, restore, or purge operations.
        /// </summary>
        public async Task<List<TEntity>> GetDeletedListAsync<TEntity>(
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> includes = null,
            bool asNoTracking = false,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            var query = _dbContext.Set<TEntity>().Where(e => e.IsDeleted);

            if (includes != null)
                query = includes(query);

            if (asNoTracking)
                query = query.AsNoTracking();

            return await query.ToListAsync(cancellationToken);
        }

        #endregion GetDeletedListAsync (Audit / Restore)

        #region TryGetListAsync (Safe Access)

        /// <summary>
        /// Attempts to retrieve a list of entities without throwing.
        /// </summary>
        /// <returns>True and the list if successful; otherwise, False and empty list.</returns>
        public async Task<(bool Success, List<TEntity> Items)> TryGetListAsync<TEntity>(
            Expression<Func<TEntity, bool>> condition = null,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            try
            {
                var items = condition == null
                    ? await GetListAsync<TEntity>(cancellationToken)
                    : await GetListAsync(condition, cancellationToken);

                return (true, items);
            }
            catch
            {
                return (false, new List<TEntity>());
            }
        }

        #endregion TryGetListAsync (Safe Access)

        #region GetDistinctByAsync (Dropdowns / Filters)

        /// <summary>
        /// Retrieves distinct values of a property (e.g., statuses, categories).
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <typeparam name="TKey">The property type (e.g., string, int).</typeparam>
        /// <param name="keySelector">The property to extract (e.g., u => u.Status).</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>A list of distinct non-null values.</returns>
        public async Task<List<TKey>> GetDistinctByAsync<TEntity, TKey>(
            Expression<Func<TEntity, TKey>> keySelector,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));

            return await _dbContext.Set<TEntity>()
                .Where(e => !e.IsDeleted)
                .Select(keySelector)
                .Where(x => x != null)
                .Distinct()
                .ToListAsync(cancellationToken);
        }

        #endregion GetDistinctByAsync (Dropdowns / Filters)

        #region ExistsAnyAndListAsync (Optimized Load)

        /// <summary>
        /// Checks if any entities exist and returns the list if so.
        /// Reduces database roundtrips for "has items + load" scenarios.
        /// </summary>
        public async Task<(bool HasAny, List<TEntity> Items)> ExistsAnyAndListAsync<TEntity>(
            Expression<Func<TEntity, bool>> condition = null,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            var query = _dbContext.Set<TEntity>().Where(e => !e.IsDeleted);

            if (condition != null)
                query = query.Where(condition);

            var items = await query.ToListAsync(cancellationToken);
            return (items.Any(), items);
        }

        #endregion ExistsAnyAndListAsync (Optimized Load)
    }
}