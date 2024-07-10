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
        public async Task<int> GetCountAsync<TEntity>(CancellationToken cancellationToken = default)
                               where TEntity : BaseEntity
        {
            int count = await _dbContext.Set<TEntity>().CountAsync(cancellationToken).ConfigureAwait(false);
            return count;
        }

        /// <summary>
        /// Asynchronously retrieves the count of entities of the specified type in the database that satisfy the given condition.
        /// </summary>
        /// <typeparam name="T">The type of the entity to count.</typeparam>
        /// <param name="condition">The condition to filter entities.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the count of entities.</returns>
        public async Task<int> GetCountAsync<TEntity>(Expression<Func<TEntity, bool>> condition,
                                                CancellationToken cancellationToken = default)
                               where TEntity : BaseEntity
        {
            IQueryable<TEntity> query = _dbContext.Set<TEntity>();

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
        public async Task<int> GetCountAsync<TEntity>(IEnumerable<Expression<Func<TEntity, bool>>> conditions,
                                                CancellationToken cancellationToken = default)
                               where TEntity : BaseEntity
        {
            IQueryable<TEntity> query = _dbContext.Set<TEntity>();

            if (conditions != null)
            {
                foreach (Expression<Func<TEntity, bool>> expression in conditions)
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
        public async Task<long> GetLongCountAsync<TEntity>(CancellationToken cancellationToken = default)
                                where TEntity : BaseEntity
        {
            long count = await _dbContext.Set<TEntity>().LongCountAsync(cancellationToken).ConfigureAwait(false);
            return count;
        }

        /// <summary>
        /// Asynchronously retrieves the long count of entities of the specified type in the database that satisfy the given condition.
        /// </summary>
        /// <typeparam name="T">The type of the entity to count.</typeparam>
        /// <param name="condition">The condition to filter entities.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the long count of entities.</returns>
        public async Task<long> GetLongCountAsync<TEntity>(Expression<Func<TEntity, bool>> condition,
                                                     CancellationToken cancellationToken = default)
                                where TEntity : BaseEntity
        {
            IQueryable<TEntity> query = _dbContext.Set<TEntity>();

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
        public async Task<long> GetLongCountAsync<TEntity>(IEnumerable<Expression<Func<TEntity, bool>>> conditions,
                                                     CancellationToken cancellationToken = default)
                                where TEntity : BaseEntity
        {
            IQueryable<TEntity> query = _dbContext.Set<TEntity>();

            if (conditions != null)
            {
                foreach (Expression<Func<TEntity, bool>> expression in conditions)
                {
                    query = query.Where(expression);
                }
            }

            return await query.LongCountAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}