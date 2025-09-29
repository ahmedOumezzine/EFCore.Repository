using AhmedOumezzine.EFCore.Repository.Entities;
using AhmedOumezzine.EFCore.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AhmedOumezzine.EFCore.Repository.Repository
{
    /// <summary>
    /// Partial implementation of the generic repository for counting entities.
    /// Supports filtering, soft delete exclusion, and optimized queries.
    /// </summary>
    /// <typeparam name="TDbContext">The type of the database context.</typeparam>
    public sealed partial class Repository<TDbContext> : IRepository
        where TDbContext : DbContext
    {
        #region Count (int)

        /// <summary>
        /// Gets the count of non-deleted entities.
        /// </summary>
        public async Task<int> GetCountAsync<TEntity>(CancellationToken ct = default)
            where TEntity : BaseEntity
        {
            return await _dbContext.Set<TEntity>()
                .Where(e => !e.IsDeleted)
                .AsNoTracking()
                .CountAsync(ct);
        }

        /// <summary>
        /// Gets the count of non-deleted entities matching a condition.
        /// </summary>
        public async Task<int> GetCountAsync<TEntity>(
            Expression<Func<TEntity, bool>> condition,
            CancellationToken ct = default)
            where TEntity : BaseEntity
        {
            if (condition == null)
                return await GetCountAsync<TEntity>(ct);

            return await _dbContext.Set<TEntity>()
                .Where(e => !e.IsDeleted)
                .Where(condition)
                .AsNoTracking()
                .CountAsync(ct);
        }

        /// <summary>
        /// Gets the count of non-deleted entities matching multiple conditions.
        /// </summary>
        public async Task<int> GetCountAsync<TEntity>(
            IEnumerable<Expression<Func<TEntity, bool>>> conditions,
            CancellationToken ct = default)
            where TEntity : BaseEntity
        {
            var query = _dbContext.Set<TEntity>()
                .Where(e => !e.IsDeleted)
                .AsNoTracking();

            if (conditions != null)
            {
                foreach (var condition in conditions.Where(c => c != null))
                {
                    query = query.Where(condition);
                }
            }

            return await query.CountAsync(ct);
        }

        #endregion Count (int)

        #region LongCount (long)

        /// <summary>
        /// Gets the long count of non-deleted entities.
        /// </summary>
        public async Task<long> GetLongCountAsync<TEntity>(CancellationToken ct = default)
            where TEntity : BaseEntity
        {
            return await _dbContext.Set<TEntity>()
                .Where(e => !e.IsDeleted)
                .AsNoTracking()
                .LongCountAsync(ct);
        }

        /// <summary>
        /// Gets the long count of non-deleted entities matching a condition.
        /// </summary>
        public async Task<long> GetLongCountAsync<TEntity>(
            Expression<Func<TEntity, bool>> condition,
            CancellationToken ct = default)
            where TEntity : BaseEntity
        {
            if (condition == null)
                return await GetLongCountAsync<TEntity>(ct);

            return await _dbContext.Set<TEntity>()
                .Where(e => !e.IsDeleted)
                .Where(condition)
                .AsNoTracking()
                .LongCountAsync(ct);
        }

        /// <summary>
        /// Gets the long count of non-deleted entities matching multiple conditions.
        /// </summary>
        public async Task<long> GetLongCountAsync<TEntity>(
            IEnumerable<Expression<Func<TEntity, bool>>> conditions,
            CancellationToken ct = default)
            where TEntity : BaseEntity
        {
            var query = _dbContext.Set<TEntity>()
                .Where(e => !e.IsDeleted)
                .AsNoTracking();

            if (conditions != null)
            {
                foreach (var condition in conditions.Where(c => c != null))
                {
                    query = query.Where(condition);
                }
            }

            return await query.LongCountAsync(ct);
        }

        #endregion LongCount (long)

        #region Existence Checks

        /// <summary>
        /// Checks if any non-deleted entity exists.
        /// More efficient than Count > 0.
        /// </summary>
        public async Task<bool> HasAnyAsync<TEntity>(CancellationToken ct = default)
            where TEntity : BaseEntity
        {
            return await _dbContext.Set<TEntity>()
                .AnyAsync(e => !e.IsDeleted, ct);
        }

        /// <summary>
        /// Checks if any non-deleted entity matches the condition.
        /// </summary>
        public async Task<bool> HasAnyAsync<TEntity>(
            Expression<Func<TEntity, bool>> condition,
            CancellationToken ct = default)
            where TEntity : BaseEntity
        {
            if (condition == null)
                return await HasAnyAsync<TEntity>(ct);

            return await _dbContext.Set<TEntity>()
                .Where(e => !e.IsDeleted)
                .AnyAsync(condition, ct);
        }

        #endregion Existence Checks

        #region Soft-Delete Statistics

        /// <summary>
        /// Counts the number of soft-deleted entities.
        /// </summary>
        public async Task<int> CountSoftDeletedAsync<TEntity>(CancellationToken ct = default)
            where TEntity : BaseEntity
        {
            return await _dbContext.Set<TEntity>()
                .CountAsync(e => e.IsDeleted, ct);
        }

        #endregion Soft-Delete Statistics

        #region Analytics & Grouping

        /// <summary>
        /// Counts entities grouped by a boolean status (e.g., IsActive).
        /// </summary>
        public async Task<Dictionary<bool, int>> CountByStatusAsync<TEntity>(
            Expression<Func<TEntity, bool>> statusSelector,
            CancellationToken ct = default)
            where TEntity : BaseEntity
        {
            return await _dbContext.Set<TEntity>()
                .Where(e => !e.IsDeleted)
                .GroupBy(statusSelector)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count, ct);
        }

        /// <summary>
        /// Counts entities created within a UTC date range (inclusive).
        /// </summary>
        public async Task<int> CountByDateRangeAsync<TEntity>(
            DateTime startDate,
            DateTime endDate,
            CancellationToken ct = default)
            where TEntity : BaseEntity
        {
            return await _dbContext.Set<TEntity>()
                .CountAsync(e => !e.IsDeleted &&
                                 e.CreatedOnUtc >= startDate &&
                                 e.CreatedOnUtc <= endDate, ct);
        }

        #endregion Analytics & Grouping
    }
}