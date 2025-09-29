using AhmedOumezzine.EFCore.Repository.Entities;
using AhmedOumezzine.EFCore.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace AhmedOumezzine.EFCore.Repository.Repository
{
    /// <summary>
    /// Partial implementation of the generic repository for retrieving entities by primary key (Guid).
    /// Automatically excludes soft-deleted entities.
    /// </summary>
    public sealed partial class Repository<TDbContext> : IRepository
        where TDbContext : DbContext
    {
        #region GetByIdAsync - Main Overloads (Guid only)

        /// <summary>
        /// Retrieves an entity by its Guid primary key.
        /// </summary>
        public async Task<TEntity?> GetByIdAsync<TEntity>(
            Guid id,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            return await _dbContext.Set<TEntity>()
                .Where(e => e.Id == id && !e.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Retrieves an entity by its Guid primary key with includes.
        /// </summary>
        public async Task<TEntity?> GetByIdAsync<TEntity>(
            Guid id,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> includes,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            if (includes == null) throw new ArgumentNullException(nameof(includes));

            var query = _dbContext.Set<TEntity>().Where(e => e.Id == id && !e.IsDeleted);
            query = includes(query);
            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Retrieves an entity by its Guid primary key with optional tracking.
        /// </summary>
        public async Task<TEntity?> GetByIdAsync<TEntity>(
            Guid id,
            bool asNoTracking,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            var query = _dbContext.Set<TEntity>().Where(e => e.Id == id && !e.IsDeleted);
            if (asNoTracking) query = query.AsNoTracking();
            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Retrieves an entity by its Guid primary key with includes and optional tracking.
        /// </summary>
        public async Task<TEntity?> GetByIdAsync<TEntity>(
            Guid id,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> includes,
            bool asNoTracking,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            if (includes == null) throw new ArgumentNullException(nameof(includes));

            var query = _dbContext.Set<TEntity>().Where(e => e.Id == id && !e.IsDeleted);
            query = includes(query);
            if (asNoTracking) query = query.AsNoTracking();
            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        #endregion GetByIdAsync - Main Overloads (Guid only)

        #region Projection Overloads

        /// <summary>
        /// Projects and retrieves a single value by Guid ID.
        /// </summary>
        public async Task<TProjected?> GetProjectedByIdAsync<TEntity, TProjected>(
            Guid id,
            Expression<Func<TEntity, TProjected>> selector,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
            where TProjected : class
        {
            if (selector == null) throw new ArgumentNullException(nameof(selector));

            return await _dbContext.Set<TEntity>()
                .Where(e => e.Id == id && !e.IsDeleted)
                .Select(selector)
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Retrieves a single property value by Guid ID.
        /// </summary>
        public async Task<TProperty> GetPropertyByIdAsync<TEntity, TProperty>(
            Guid id,
            Expression<Func<TEntity, TProperty>> propertySelector,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            if (propertySelector == null) throw new ArgumentNullException(nameof(propertySelector));

            return await _dbContext.Set<TEntity>()
                .Where(e => e.Id == id && !e.IsDeleted)
                .Select(propertySelector)
                .FirstOrDefaultAsync(cancellationToken);
        }

        #endregion Projection Overloads

        #region Batch Load (GetByIdsAsync)

        /// <summary>
        /// Retrieves multiple entities by their Guid IDs.
        /// </summary>
        public async Task<List<TEntity>> GetByIdsAsync<TEntity>(
            IEnumerable<Guid> ids,
            bool asNoTracking = false,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            if (ids == null) throw new ArgumentNullException(nameof(ids));

            var idList = ids.ToList();
            if (!idList.Any()) return new List<TEntity>();

            var query = _dbContext.Set<TEntity>()
                .Where(e => idList.Contains(e.Id) && !e.IsDeleted);

            if (asNoTracking)
                query = query.AsNoTracking();

            return await query.ToListAsync(cancellationToken);
        }

        #endregion Batch Load (GetByIdsAsync)

        #region Safe Access & Aliases

        /// <summary>
        /// Alias for GetByIdAsync (semantic clarity).
        /// </summary>
        public Task<TEntity?> FindByIdAsync<TEntity>(Guid id, CancellationToken ct = default)
            where TEntity : BaseEntity
            => GetByIdAsync<TEntity>(id, ct);

        /// <summary>
        /// Attempts to get an entity without throwing.
        /// </summary>
        public async Task<(bool Success, TEntity? Entity)> TryGetByIdAsync<TEntity>(
            Guid id,
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

        #endregion Safe Access & Aliases
    }
}