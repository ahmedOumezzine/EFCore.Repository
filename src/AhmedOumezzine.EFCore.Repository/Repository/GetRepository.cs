using AhmedOumezzine.EFCore.Repository.Entities;
using AhmedOumezzine.EFCore.Repository.Interface;
using AhmedOumezzine.EFCore.Repository.Specification;
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
        /// Asynchronously retrieves an entity of the specified type from the database based on the provided condition.
        /// </summary>
        /// <typeparam name="T">The type of entity to retrieve.</typeparam>
        /// <param name="condition">The condition to filter entities.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the first entity matching the condition, or null if none is found.</returns>
        public Task<T> GetAsync<T>(
            Expression<Func<T, bool>> condition,
            CancellationToken cancellationToken = default) where T : BaseEntity
        {
            return GetAsync(condition, null, false, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves an entity of the specified type from the database based on the provided condition and tracking option.
        /// </summary>
        /// <typeparam name="T">The type of entity to retrieve.</typeparam>
        /// <param name="condition">The condition to filter entities.</param>
        /// <param name="asNoTracking">Indicates whether to track changes for the retrieved entity.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the first entity matching the condition, or null if none is found.</returns>
        public Task<T> GetAsync<T>(
            Expression<Func<T, bool>> condition,
            bool asNoTracking,
            CancellationToken cancellationToken = default) where T : BaseEntity
        {
            return GetAsync(condition, null, asNoTracking, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves an entity of the specified type from the database based on the provided condition and including related entities.
        /// </summary>
        /// <typeparam name="T">The type of entity to retrieve.</typeparam>
        /// <param name="condition">The condition to filter entities.</param>
        /// <param name="includes">A function to include related entities in the query.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the first entity matching the condition, or null if none is found.</returns>
        public Task<T> GetAsync<T>(
            Expression<Func<T, bool>> condition,
            Func<IQueryable<T>, IIncludableQueryable<T, object>> includes,
            CancellationToken cancellationToken = default) where T : BaseEntity
        {
            return GetAsync(condition, includes, false, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves an entity of the specified type from the database based on the provided condition, including related entities, and tracking option.
        /// </summary>
        /// <typeparam name="T">The type of entity to retrieve.</typeparam>
        /// <param name="condition">The condition to filter entities.</param>
        /// <param name="includes">A function to include related entities in the query.</param>
        /// <param name="asNoTracking">Indicates whether to track changes for the retrieved entity.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the first entity matching the condition, or null if none is found.</returns>
        public async Task<T> GetAsync<T>(
            Expression<Func<T, bool>> condition,
            Func<IQueryable<T>, IIncludableQueryable<T, object>> includes,
            bool asNoTracking,
            CancellationToken cancellationToken = default) where T : BaseEntity
        {
            IQueryable<T> query = _dbContext.Set<T>();

            if (condition != null)
            {
                query = query.Where(condition);
            }

            if (includes != null)
            {
                query = includes(query);
            }

            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            return await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously retrieves an entity of the specified type from the database based on the provided specification.
        /// </summary>
        /// <typeparam name="T">The type of entity to retrieve.</typeparam>
        /// <param name="specification">The specification to apply to the query.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the first entity matching the specification, or null if none is found.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the provided specification is null.</exception>
        public Task<T> GetAsync<T>(Specification<T> specification, CancellationToken cancellationToken = default) where T : BaseEntity
        {
            return GetAsync(specification, false, cancellationToken);
        }

        public async Task<T> GetAsync<T>(Specification<T> specification, bool asNoTracking, CancellationToken cancellationToken = default) where T : BaseEntity
        {
            IQueryable<T> query = _dbContext.Set<T>();

            if (specification != null)
            {
                query = query.GetSpecifiedQuery(specification);
            }

            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            return await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously retrieves an entity of the specified type from the database based on the provided specification and tracking option.
        /// </summary>
        /// <typeparam name="T">The type of entity to retrieve.</typeparam>
        /// <param name="specification">The specification to apply to the query.</param>
        /// <param name="asNoTracking">Indicates whether to track changes for the retrieved entity.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the first entity matching the specification, or null if none is found.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the provided specification is null.</exception>
        public async Task<TProjectedType> GetAsync<T, TProjectedType>(
            Expression<Func<T, bool>> condition,
            Expression<Func<T, TProjectedType>> selectExpression,
            CancellationToken cancellationToken = default) where T : BaseEntity
        {
            if (selectExpression == null)
            {
                throw new ArgumentNullException(nameof(selectExpression));
            }

            IQueryable<T> query = _dbContext.Set<T>();

            if (condition != null)
            {
                query = query.Where(condition);
            }

            return await query.Select(selectExpression).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously retrieves a projected entity of the specified type from the database based on the provided condition and projection expression.
        /// </summary>
        /// <typeparam name="T">The type of entity to retrieve.</typeparam>
        /// <typeparam name="TProjectedType">The type of projected entity.</typeparam>
        /// <param name="condition">The condition to filter entities.</param>
        /// <param name="selectExpression">The expression to select and project the entity.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the projected entity matching the condition, or default(TProjectedType) if none is found.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the provided select expression is null.</exception>
        public async Task<TProjectedType> GetAsync<T, TProjectedType>(
                                                            Specification<T> specification,
                                                            Expression<Func<T, TProjectedType>> selectExpression,
                                                            CancellationToken cancellationToken = default) where T : BaseEntity
        {
            if (selectExpression == null)
            {
                throw new ArgumentNullException(nameof(selectExpression));
            }

            IQueryable<T> query = _dbContext.Set<T>();

            if (specification != null)
            {
                query = query.GetSpecifiedQuery(specification);
            }

            return await query.Select(selectExpression).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}