using AhmedOumezzine.EFCore.Repository.Entities;
using AhmedOumezzine.EFCore.Repository.Interface;
using AhmedOumezzine.EFCore.Repository.Specification;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace AhmedOumezzine.EFCore.Repository.Repository
{
    /// <summary>
    /// Partial implementation of the generic repository for retrieving a single entity.
    /// Supports filtering, projection, includes, specifications, and safe operations.
    /// Automatically excludes soft-deleted entities.
    /// </summary>
    public sealed partial class Repository<TDbContext> : IRepository
        where TDbContext : DbContext
    {
        #region GetAsync - By Condition

        public Task<TEntity?> GetAsync<TEntity>(
            Expression<Func<TEntity, bool>> condition,
            CancellationToken ct = default)
            where TEntity : BaseEntity
            => GetAsync(condition, includes: null, asNoTracking: false, ct);

        public Task<TEntity?> GetAsync<TEntity>(
            Expression<Func<TEntity, bool>> condition,
            bool asNoTracking,
            CancellationToken ct = default)
            where TEntity : BaseEntity
            => GetAsync(condition, includes: null, asNoTracking: asNoTracking, ct);

        public Task<TEntity?> GetAsync<TEntity>(
            Expression<Func<TEntity, bool>> condition,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> includes,
            CancellationToken ct = default)
            where TEntity : BaseEntity
            => GetAsync(condition, includes, asNoTracking: false, ct);

        public async Task<TEntity?> GetAsync<TEntity>(
            Expression<Func<TEntity, bool>> condition,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> includes,
            bool asNoTracking,
            CancellationToken ct = default)
            where TEntity : BaseEntity
        {
            if (condition == null) throw new ArgumentNullException(nameof(condition));

            var query = _dbContext.Set<TEntity>().Where(e => !e.IsDeleted);
            query = query.Where(condition);

            if (includes != null)
                query = includes(query);

            if (asNoTracking)
                query = query.AsNoTracking();

            return await query.FirstOrDefaultAsync(ct);
        }

        #endregion GetAsync - By Condition

        #region GetAsync - By Specification

        public Task<TEntity?> GetAsync<TEntity>(
            Specification<TEntity> specification,
            CancellationToken ct = default)
            where TEntity : BaseEntity
            => GetAsync(specification, asNoTracking: false, ct);

        public async Task<TEntity?> GetAsync<TEntity>(
            Specification<TEntity> specification,
            bool asNoTracking,
            CancellationToken ct = default)
            where TEntity : BaseEntity
        {
            if (specification == null) throw new ArgumentNullException(nameof(specification));

            // ✅ Appliquer le soft-delete AVANT la spécification
            var query = _dbContext.Set<TEntity>().Where(e => !e.IsDeleted);
            query = query.GetSpecifiedQuery(specification);

            if (asNoTracking)
                query = query.AsNoTracking();

            return await query.FirstOrDefaultAsync(ct);
        }

        #endregion GetAsync - By Specification

        #region GetAsync - Projection

        public async Task<TProjected?> GetAsync<TEntity, TProjected>(
            Expression<Func<TEntity, bool>> condition,
            Expression<Func<TEntity, TProjected>> selector,
            CancellationToken ct = default)
            where TEntity : BaseEntity
            where TProjected : class
        {
            if (condition == null) throw new ArgumentNullException(nameof(condition));
            if (selector == null) throw new ArgumentNullException(nameof(selector));

            return await _dbContext.Set<TEntity>()
                .Where(e => !e.IsDeleted)
                .Where(condition)
                .Select(selector)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<TProjected?> GetAsync<TEntity, TProjected>(
            Specification<TEntity> specification,
            Expression<Func<TEntity, TProjected>> selector,
            CancellationToken ct = default)
            where TEntity : BaseEntity
            where TProjected : class
        {
            if (specification == null) throw new ArgumentNullException(nameof(specification));
            if (selector == null) throw new ArgumentNullException(nameof(selector));

            var query = _dbContext.Set<TEntity>().Where(e => !e.IsDeleted);
            query = query.GetSpecifiedQuery(specification);

            return await query.Select(selector).FirstOrDefaultAsync(ct);
        }

        #endregion GetAsync - Projection

        #region Safe & Utility Methods

        public async Task<(bool Success, TEntity? Entity)> TryGetAsync<TEntity>(
            Expression<Func<TEntity, bool>> condition,
            CancellationToken ct = default)
            where TEntity : BaseEntity
        {
            try
            {
                var entity = await GetAsync(condition, ct);
                return (entity != null, entity);
            }
            catch
            {
                return (false, null);
            }
        }

        /// <summary>
        /// Alias for semantic clarity.
        /// </summary>
        public Task<TEntity?> GetAsyncOrDefault<TEntity>(
            Expression<Func<TEntity, bool>> condition,
            CancellationToken ct = default)
            where TEntity : BaseEntity
            => GetAsync(condition, ct);

        /// <summary>
        /// Alias for semantic clarity.
        /// </summary>
        public Task<TEntity?> FindAsync<TEntity>(
            Expression<Func<TEntity, bool>> condition,
            CancellationToken ct = default)
            where TEntity : BaseEntity
            => GetAsync(condition, ct);

        /// <summary>
        /// Throws if entity not found.
        /// </summary>
        public async Task<TEntity> GetFirstOrThrowAsync<TEntity>(
            Expression<Func<TEntity, bool>> condition,
            string? errorMessage = null,
            CancellationToken ct = default)
            where TEntity : BaseEntity
        {
            var entity = await GetAsync(condition, ct);
            if (entity == null)
                throw new InvalidOperationException(errorMessage ?? $"Entity of type {typeof(TEntity).Name} not found.");
            return entity;
        }

        /// <summary>
        /// Checks existence and returns entity in one call.
        /// </summary>
        public async Task<(bool Exists, TEntity? Entity)> ExistsAndFetchAsync<TEntity>(
            Expression<Func<TEntity, bool>> condition,
            CancellationToken ct = default)
            where TEntity : BaseEntity
        {
            var entity = await GetAsync(condition, ct);
            return (entity != null, entity);
        }

        /// <summary>
        /// Retrieves a single property value.
        /// </summary>
        public async Task<TProperty> GetOnlyAsync<TEntity, TProperty>(
            Expression<Func<TEntity, bool>> condition,
            Expression<Func<TEntity, TProperty>> propertySelector,
            CancellationToken ct = default)
            where TEntity : BaseEntity
        {
            if (condition == null) throw new ArgumentNullException(nameof(condition));
            if (propertySelector == null) throw new ArgumentNullException(nameof(propertySelector));

            return await _dbContext.Set<TEntity>()
                .Where(e => !e.IsDeleted)
                .Where(condition)
                .Select(propertySelector)
                .FirstOrDefaultAsync(ct);
        }

        #endregion Safe & Utility Methods
    }
}