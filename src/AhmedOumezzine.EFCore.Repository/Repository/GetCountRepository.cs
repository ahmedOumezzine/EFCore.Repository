using AhmedOumezzine.EFCore.Repository.Entities;
using AhmedOumezzine.EFCore.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq.Expressions;

namespace AhmedOumezzine.EFCore.Repository.Repository
{
    public sealed partial class Repository<TDbContext> : IRepository
        where TDbContext : DbContext
    {
        /// <summary>
        /// Asynchronously retrieves the count of entities of the specified type in the database.
        /// </summary>
        /// <typeparam name="T">The type of the entity to count.</typeparam>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the count of entities.</returns>
        public async Task<int> GetCountAsync<T>(CancellationToken cancellationToken = default) where T : BaseEntity
        {
            int count = await _dbContext.Set<T>().CountAsync(cancellationToken).ConfigureAwait(false);
            return count;
        }

        /// <summary>
        /// Asynchronously retrieves the count of entities of the specified type in the database that satisfy the given condition.
        /// </summary>
        /// <typeparam name="T">The type of the entity to count.</typeparam>
        /// <param name="condition">The condition to filter entities.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the count of entities.</returns>
        public async Task<int> GetCountAsync<T>(Expression<Func<T, bool>> condition, CancellationToken cancellationToken = default) where T : BaseEntity
        {
            IQueryable<T> query = _dbContext.Set<T>();

            if (condition != null)
            {
                query = query.Where(condition);
            }

            return await query.CountAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously retrieves the count of entities of the specified type in the database that satisfy all the given conditions.
        /// </summary>
        /// <typeparam name="T">The type of the entity to count.</typeparam>
        /// <param name="conditions">The list of conditions to filter entities.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the count of entities.</returns>
        public async Task<int> GetCountAsync<T>(IEnumerable<Expression<Func<T, bool>>> conditions, CancellationToken cancellationToken = default) where T : BaseEntity
        {
            IQueryable<T> query = _dbContext.Set<T>();

            if (conditions != null)
            {
                foreach (Expression<Func<T, bool>> expression in conditions)
                {
                    query = query.Where(expression);
                }
            }

            return await query.CountAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously retrieves the long count of entities of the specified type in the database.
        /// </summary>
        /// <typeparam name="T">The type of the entity to count.</typeparam>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the long count of entities.</returns>
        public async Task<long> GetLongCountAsync<T>(CancellationToken cancellationToken = default) where T : BaseEntity
        {
            long count = await _dbContext.Set<T>().LongCountAsync(cancellationToken).ConfigureAwait(false);
            return count;
        }

        /// <summary>
        /// Asynchronously retrieves the long count of entities of the specified type in the database that satisfy the given condition.
        /// </summary>
        /// <typeparam name="T">The type of the entity to count.</typeparam>
        /// <param name="condition">The condition to filter entities.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the long count of entities.</returns>
        public async Task<long> GetLongCountAsync<T>(Expression<Func<T, bool>> condition, CancellationToken cancellationToken = default) where T : BaseEntity
        {
            IQueryable<T> query = _dbContext.Set<T>();

            if (condition != null)
            {
                query = query.Where(condition);
            }

            return await query.LongCountAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously retrieves the long count of entities of the specified type in the database that satisfy all the given conditions.
        /// </summary>
        /// <typeparam name="T">The type of the entity to count.</typeparam>
        /// <param name="conditions">The list of conditions to filter entities.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the long count of entities.</returns>
        public async Task<long> GetLongCountAsync<T>(IEnumerable<Expression<Func<T, bool>>> conditions, CancellationToken cancellationToken = default) where T : BaseEntity
        {
            IQueryable<T> query = _dbContext.Set<T>();

            if (conditions != null)
            {
                foreach (Expression<Func<T, bool>> expression in conditions)
                {
                    query = query.Where(expression);
                }
            }

            return await query.LongCountAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}