using AhmedOumezzine.EFCore.Repository.Entities;
using AhmedOumezzine.EFCore.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Data;
using System.Linq.Expressions;

namespace AhmedOumezzine.EFCore.Repository.Repository
{
    public sealed partial class Repository<TDbContext> : IRepository
         where TDbContext : DbContext
    {
        /// <summary>
        /// Asynchronously retrieves an entity of the specified type by its primary key.
        /// </summary>
        /// <typeparam name="T">The type of the entity to retrieve.</typeparam>
        /// <param name="id">The primary key value of the entity to retrieve.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the entity if found; otherwise, null.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the provided primary key value is null.</exception>
        public Task<TEntity> GetByIdAsync<TEntity>(Guid? id,
                                       CancellationToken cancellationToken = default)
                       where TEntity : BaseEntity
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            return GetByIdAsync<TEntity>(id.Value, false, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves an entity of the specified type by its primary key, optionally tracking the entity.
        /// </summary>
        /// <typeparam name="T">The type of the entity to retrieve.</typeparam>
        /// <param name="id">The primary key value of the entity to retrieve.</param>
        /// <param name="asNoTracking">Indicates whether to disable entity tracking for the retrieved entity.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the entity if found; otherwise, null.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the provided primary key value is null.</exception>
        public Task<TEntity> GetByIdAsync<TEntity>(Guid? id,
                                       bool asNoTracking,
                                       CancellationToken cancellationToken = default)
                       where TEntity : BaseEntity
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            return GetByIdAsync<TEntity>(id.Value, null, asNoTracking, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves an entity of the specified type by its primary key with optional includes.
        /// </summary>
        /// <typeparam name="T">The type of the entity to retrieve.</typeparam>
        /// <param name="id">The primary key value of the entity to retrieve.</param>
        /// <param name="includes">An optional function to include related entities in the query.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the entity if found; otherwise, null.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the provided primary key value is null.</exception>
        public Task<TEntity> GetByIdAsync<TEntity>(Guid? id,
                                       Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> includes,
                                       CancellationToken cancellationToken = default)
                       where TEntity : BaseEntity
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            return GetByIdAsync(id.Value, includes, false, cancellationToken);
        }

        // <summary>
        /// Asynchronously retrieves an entity of the specified type by its primary key with optional includes and tracking.
        /// </summary>
        /// <typeparam name="T">The type of the entity to retrieve.</typeparam>
        /// <param name="id">The primary key value of the entity to retrieve.</param>
        /// <param name="includes">An optional function to include related entities in the query.</param>
        /// <param name="asNoTracking">Indicates whether to disable entity tracking for the retrieved entity.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the entity if found; otherwise, null.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the provided primary key value is null.</exception>

        public async Task<TEntity> GetByIdAsync<TEntity>(Guid? id,
                                             Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> includes,
                                             bool asNoTracking = false,
                                             CancellationToken cancellationToken = default)
                             where TEntity : BaseEntity
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }
            Expression<Func<TEntity, bool>> expressionTree = x => x.Id == id && x.IsDeleted == false;

            IQueryable<TEntity> query = _dbContext.Set<TEntity>();

            if (includes != null)
            {
                query = includes(query);
            }

            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            return await query.FirstOrDefaultAsync(expressionTree, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously retrieves a projected entity of the specified type by its primary key.
        /// </summary>
        /// <typeparam name="T">The type of the entity to retrieve.</typeparam>
        /// <typeparam name="TProjectedType">The type of the projection.</typeparam>
        /// <param name="id">The primary key value of the entity to retrieve.</param>
        /// <param name="selectExpression">The expression representing the projection.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the projected entity if found; otherwise, null.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the provided primary key value or projection expression is null.</exception>

        public async Task<TProjectedType> GetByIdAsync<TEntity, TProjectedType>(Guid? id,
                                                                          Expression<Func<TEntity, TProjectedType>> selectExpression,
                                                                          CancellationToken cancellationToken = default)
                                          where TEntity : BaseEntity
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (selectExpression == null)
            {
                throw new ArgumentNullException(nameof(selectExpression));
            }
            Expression<Func<TEntity, bool>> expressionTree = x => x.Id == id && x.IsDeleted == false;

            IQueryable<TEntity> query = _dbContext.Set<TEntity>();

            return await query.Where(expressionTree)
                              .Select(selectExpression)
                              .FirstOrDefaultAsync(cancellationToken)
                              .ConfigureAwait(false);
        }
    }
}