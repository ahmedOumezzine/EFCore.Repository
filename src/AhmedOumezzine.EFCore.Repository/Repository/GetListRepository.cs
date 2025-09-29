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
    public sealed partial class Repository<TDbContext> : IRepository
        where TDbContext : DbContext
    {
        #region GetListAsync - Base Overloads

        public Task<List<TEntity>> GetListAsync<TEntity>(CancellationToken ct = default)
            where TEntity : BaseEntity
            => GetListAsync<TEntity>(includes: null, asNoTracking: false, ct);

        public Task<List<TEntity>> GetListAsync<TEntity>(bool asNoTracking, CancellationToken ct = default)
            where TEntity : BaseEntity
            => GetListAsync<TEntity>(includes: null, asNoTracking: asNoTracking, ct);

        public Task<List<TEntity>> GetListAsync<TEntity>(
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> includes,
            CancellationToken ct = default)
            where TEntity : BaseEntity
            => GetListAsync(includes, asNoTracking: false, ct);

        public async Task<List<TEntity>> GetListAsync<TEntity>(
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> includes,
            bool asNoTracking,
            CancellationToken ct = default)
            where TEntity : BaseEntity
        {
            var query = _dbContext.Set<TEntity>().Where(e => !e.IsDeleted);

            if (includes != null)
                query = includes(query);

            if (asNoTracking)
                query = query.AsNoTracking();

            return await query.ToListAsync(ct);
        }

        #endregion GetListAsync - Base Overloads

        #region GetListAsync - With Condition

        public Task<List<TEntity>> GetListAsync<TEntity>(
            Expression<Func<TEntity, bool>> condition,
            CancellationToken ct = default)
            where TEntity : BaseEntity
            => GetListAsync(condition, includes: null, asNoTracking: false, ct);

        public Task<List<TEntity>> GetListAsync<TEntity>(
            Expression<Func<TEntity, bool>> condition,
            bool asNoTracking,
            CancellationToken ct = default)
            where TEntity : BaseEntity
            => GetListAsync(condition, includes: null, asNoTracking: asNoTracking, ct);

        public async Task<List<TEntity>> GetListAsync<TEntity>(
            Expression<Func<TEntity, bool>> condition,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> includes,
            bool asNoTracking,
            CancellationToken ct = default)
            where TEntity : BaseEntity
        {
            var query = _dbContext.Set<TEntity>().Where(e => !e.IsDeleted);

            if (condition != null)
                query = query.Where(condition);

            if (includes != null)
                query = includes(query);

            if (asNoTracking)
                query = query.AsNoTracking();

            return await query.ToListAsync(ct);
        }

        #endregion GetListAsync - With Condition

        #region GetListAsync - With Specification

        public Task<List<TEntity>> GetListAsync<TEntity>(
            Specification<TEntity> specification,
            CancellationToken ct = default)
            where TEntity : BaseEntity
            => GetListAsync(specification, asNoTracking: false, ct);

        public async Task<List<TEntity>> GetListAsync<TEntity>(
            Specification<TEntity> specification,
            bool asNoTracking,
            CancellationToken ct = default)
            where TEntity : BaseEntity
        {
            // Appliquer le soft-delete AVANT la spécification
            var query = _dbContext.Set<TEntity>().Where(e => !e.IsDeleted);

            if (specification != null)
                query = query.GetSpecifiedQuery(specification);

            if (asNoTracking)
                query = query.AsNoTracking();

            return await query.ToListAsync(ct);
        }

        #endregion GetListAsync - With Specification

        #region GetListAsync - Projection

        public async Task<List<TProjected>> GetListAsync<TEntity, TProjected>(
            Expression<Func<TEntity, TProjected>> selector,
            CancellationToken ct = default)
            where TEntity : BaseEntity
            where TProjected : class
        {
            if (selector == null) throw new ArgumentNullException(nameof(selector));

            return await _dbContext.Set<TEntity>()
                .Where(e => !e.IsDeleted)
                .Select(selector)
                .ToListAsync(ct);
        }

        public async Task<List<TProjected>> GetListAsync<TEntity, TProjected>(
            Expression<Func<TEntity, bool>> condition,
            Expression<Func<TEntity, TProjected>> selector,
            CancellationToken ct = default)
            where TEntity : BaseEntity
            where TProjected : class
        {
            if (selector == null) throw new ArgumentNullException(nameof(selector));

            var query = _dbContext.Set<TEntity>().Where(e => !e.IsDeleted);
            if (condition != null) query = query.Where(condition);

            return await query.Select(selector).ToListAsync(ct);
        }

        public async Task<List<TProjected>> GetListAsync<TEntity, TProjected>(
            Specification<TEntity> specification,
            Expression<Func<TEntity, TProjected>> selector,
            CancellationToken ct = default)
            where TEntity : BaseEntity
            where TProjected : class
        {
            if (selector == null) throw new ArgumentNullException(nameof(selector));

            var query = _dbContext.Set<TEntity>().Where(e => !e.IsDeleted);

            if (specification != null)
                query = query.GetSpecifiedQuery(specification);

            return await query.Select(selector).ToListAsync(ct);
        }

        #endregion GetListAsync - Projection

        #region GetListAsync - Pagination

        public async Task<PaginatedList<TEntity>> GetListAsync<TEntity>(
            PaginationSpecification<TEntity> specification,
            CancellationToken ct = default)
            where TEntity : BaseEntity
        {
            if (specification == null) throw new ArgumentNullException(nameof(specification));

            var query = _dbContext.Set<TEntity>().Where(e => !e.IsDeleted);
            return await query.ToPaginatedListAsync(specification, ct);
        }

        public async Task<PaginatedList<TProjected>> GetListAsync<TEntity, TProjected>(
            PaginationSpecification<TEntity> specification,
            Expression<Func<TEntity, TProjected>> selector,
            CancellationToken ct = default)
            where TEntity : BaseEntity
            where TProjected : class
        {
            if (specification == null) throw new ArgumentNullException(nameof(specification));
            if (selector == null) throw new ArgumentNullException(nameof(selector));

            var query = _dbContext.Set<TEntity>().Where(e => !e.IsDeleted);

            if (specification != null)
                query = query.GetSpecifiedQuery((SpecificationBase<TEntity>)specification);

            return await query.Select(selector)
                .ToPaginatedListAsync(specification.PageIndex, specification.PageSize, ct);
        }

        #endregion GetListAsync - Pagination

        #region Specialized Lists

        /// <summary>
        /// Alias for GetListAsync (semantic clarity).
        /// </summary>
        public Task<List<TEntity>> GetActiveListAsync<TEntity>(CancellationToken ct = default)
            where TEntity : BaseEntity
            => GetListAsync<TEntity>(ct);

        /// <summary>
        /// Retrieves soft-deleted entities (for audit/restore).
        /// </summary>
        public async Task<List<TEntity>> GetDeletedListAsync<TEntity>(
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> includes = null,
            bool asNoTracking = true, // ✅ No tracking by default
            CancellationToken ct = default)
            where TEntity : BaseEntity
        {
            var query = _dbContext.Set<TEntity>().Where(e => e.IsDeleted);

            if (includes != null)
                query = includes(query);

            if (asNoTracking)
                query = query.AsNoTracking();

            return await query.ToListAsync(ct);
        }

        #endregion Specialized Lists

        #region Safe & Utility Methods

        public async Task<(bool Success, List<TEntity> Items)> TryGetListAsync<TEntity>(
            Expression<Func<TEntity, bool>> condition = null,
            CancellationToken ct = default)
            where TEntity : BaseEntity
        {
            try
            {
                var items = condition == null
                    ? await GetListAsync<TEntity>(ct)
                    : await GetListAsync(condition, ct);
                return (true, items);
            }
            catch
            {
                return (false, new List<TEntity>());
            }
        }

        /// <summary>
        /// Gets distinct values of a property (includes nulls if applicable).
        /// </summary>
        public async Task<List<TKey>> GetDistinctByAsync<TEntity, TKey>(
            Expression<Func<TEntity, TKey>> keySelector,
            CancellationToken ct = default)
            where TEntity : BaseEntity
        {
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));

            return await _dbContext.Set<TEntity>()
                .Where(e => !e.IsDeleted)
                .Select(keySelector)
                .Distinct()
                .ToListAsync(ct);
        }

        /// <summary>
        /// Loads the full list and checks if any items exist.
        /// Note: This loads all data — use only for small datasets.
        /// </summary>
        public async Task<(bool HasAny, List<TEntity> Items)> ExistsAnyAndListAsync<TEntity>(
            Expression<Func<TEntity, bool>> condition = null,
            CancellationToken ct = default)
            where TEntity : BaseEntity
        {
            var query = _dbContext.Set<TEntity>().Where(e => !e.IsDeleted);
            if (condition != null) query = query.Where(condition);

            var items = await query.ToListAsync(ct);
            return (items.Any(), items);
        }

        #endregion Safe & Utility Methods
    }
}