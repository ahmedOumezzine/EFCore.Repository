using AhmedOumezzine.EFCore.Repository.Entities;
using AhmedOumezzine.EFCore.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

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
  

        #region GetCountAsync (int)

        /// <summary>
        /// Asynchronously retrieves the count of non-deleted entities of the specified type.
        /// </summary>
        /// <typeparam name="TEntity">The entity type, must inherit from <see cref="BaseEntity"/>.</typeparam>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The number of non-soft-deleted entities.</returns>
        public async Task<int> GetCountAsync<TEntity>(CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            return await _dbContext.Set<TEntity>()
                .Where(e => !e.IsDeleted)
                .AsNoTracking()
                .CountAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously retrieves the count of non-deleted entities that satisfy the given condition.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="condition">The condition to filter entities.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The number of matching non-deleted entities.</returns>
        public async Task<int> GetCountAsync<TEntity>(
            Expression<Func<TEntity, bool>> condition,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            if (condition == null)
                return await GetCountAsync<TEntity>(cancellationToken);

            return await _dbContext.Set<TEntity>()
                .Where(e => !e.IsDeleted)
                .Where(condition)
                .AsNoTracking()
                .CountAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously retrieves the count of non-deleted entities that satisfy all the given conditions.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="conditions">The list of conditions to filter entities.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The number of entities matching all conditions and not soft-deleted.</returns>
        public async Task<int> GetCountAsync<TEntity>(
            IEnumerable<Expression<Func<TEntity, bool>>> conditions,
            CancellationToken cancellationToken = default)
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

            return await query.CountAsync(cancellationToken).ConfigureAwait(false);
        }

        #endregion

        #region GetLongCountAsync (long)

        /// <summary>
        /// Asynchronously retrieves the long count of non-deleted entities of the specified type.
        /// Use when the count may exceed int.MaxValue.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The number of non-soft-deleted entities as a long.</returns>
        public async Task<long> GetLongCountAsync<TEntity>(CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            return await _dbContext.Set<TEntity>()
                .Where(e => !e.IsDeleted)
                .AsNoTracking()
                .LongCountAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously retrieves the long count of non-deleted entities that satisfy the given condition.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="condition">The condition to filter entities.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The number of matching non-deleted entities as a long.</returns>
        public async Task<long> GetLongCountAsync<TEntity>(
            Expression<Func<TEntity, bool>> condition,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            if (condition == null)
                return await GetLongCountAsync<TEntity>(cancellationToken);

            return await _dbContext.Set<TEntity>()
                .Where(e => !e.IsDeleted)
                .Where(condition)
                .AsNoTracking()
                .LongCountAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously retrieves the long count of non-deleted entities that satisfy all the given conditions.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="conditions">The list of conditions to filter entities.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The number of entities matching all conditions and not soft-deleted, as a long.</returns>
        public async Task<long> GetLongCountAsync<TEntity>(
            IEnumerable<Expression<Func<TEntity, bool>>> conditions,
            CancellationToken cancellationToken = default)
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

            return await query.LongCountAsync(cancellationToken).ConfigureAwait(false);
        }

        #endregion

        #region HasAnyAsync (Existence Checks)

        /// <summary>
        /// Checks if any non-deleted entity of the specified type exists.
        /// More efficient than GetCountAsync() > 0.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>True if at least one entity exists; otherwise, false.</returns>
        public async Task<bool> HasAnyAsync<TEntity>(CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            return await _dbContext.Set<TEntity>()
                .AnyAsync(e => !e.IsDeleted, cancellationToken);
        }

        /// <summary>
        /// Checks if any non-deleted entity matches the condition.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="condition">The condition to match.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>True if a matching entity exists; otherwise, false.</returns>
        public async Task<bool> HasAnyAsync<TEntity>(
            Expression<Func<TEntity, bool>> condition,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            if (condition == null)
                return await HasAnyAsync<TEntity>(cancellationToken);

            return await _dbContext.Set<TEntity>()
                .Where(e => !e.IsDeleted)
                .AnyAsync(condition, cancellationToken);
        }

        #endregion

        #region CountDeletedAsync (Soft-Delete Audit)

        /// <summary>
        /// Counts the number of soft-deleted entities.
        /// Useful for audits or cleanup operations.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The number of soft-deleted entities.</returns>
        public async Task<int> CountDeletedAsync<TEntity>(CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            return await _dbContext.Set<TEntity>()
                .CountAsync(e => e.IsDeleted, cancellationToken);
        }

        #endregion

        #region CountByStatusAsync (Dashboard / Stats)

        /// <summary>
        /// Counts entities grouped by a boolean status (e.g., IsActive).
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="statusSelector">The property to group by (e.g., u => u.IsActive).</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>A dictionary with true/false counts.</returns>
        public async Task<Dictionary<bool, int>> CountByStatusAsync<TEntity>(
            Expression<Func<TEntity, bool>> statusSelector,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            return await _dbContext.Set<TEntity>()
                .Where(e => !e.IsDeleted)
                .GroupBy(statusSelector)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count, cancellationToken);
        }

        #endregion

        #region CountByDateRangeAsync (Analytics)

        /// <summary>
        /// Counts entities created within a date range.
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="startDate">The start of the range (inclusive).</param>
        /// <param name="endDate">The end of the range (inclusive).</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The number of entities created in the range.</returns>
        public async Task<int> CountByDateRangeAsync<TEntity>(
            DateTime startDate,
            DateTime endDate,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            return await _dbContext.Set<TEntity>()
                .CountAsync(e => !e.IsDeleted &&
                                 e.CreatedOnUtc >= startDate &&
                                 e.CreatedOnUtc <= endDate,
                           cancellationToken);
        }

        #endregion
    }
}