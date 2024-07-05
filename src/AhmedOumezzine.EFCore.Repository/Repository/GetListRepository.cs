using AhmedOumezzine.EFCore.Repository.Entities;
using AhmedOumezzine.EFCore.Repository.Extensions;
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
        /// Asynchronously retrieves a list of entities of the specified type from the database.
        /// </summary>
        /// <typeparam name="T">The type of entities to retrieve.</typeparam>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the list of entities.</returns>

        public Task<List<T>> GetListAsync<T>(CancellationToken cancellationToken = default) where T : BaseEntity
        {
            return GetListAsync<T>(false, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a list of entities of the specified type from the database, optionally without tracking.
        /// </summary>
        /// <typeparam name="T">The type of entities to retrieve.</typeparam>
        /// <param name="asNoTracking">True to retrieve entities without tracking; otherwise, false.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the list of entities.</returns>
        public Task<List<T>> GetListAsync<T>(bool asNoTracking, CancellationToken cancellationToken = default) where T : BaseEntity
        {
            Func<IQueryable<T>, IIncludableQueryable<T, object>> nullValue = null;
            return GetListAsync(nullValue, asNoTracking, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a list of entities of the specified type from the database, optionally including related entities.
        /// </summary>
        /// <typeparam name="T">The type of entities to retrieve.</typeparam>
        /// <param name="includes">A function to specify related entities to include in the query.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the list of entities.</returns>
        public Task<List<T>> GetListAsync<T>(
            Func<IQueryable<T>, IIncludableQueryable<T, object>> includes,
            CancellationToken cancellationToken = default) where T : BaseEntity
        {
            return GetListAsync(includes, false, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a list of entities of the specified type from the database, optionally including related entities and optionally without tracking.
        /// </summary>
        /// <typeparam name="T">The type of entities to retrieve.</typeparam>
        /// <param name="includes">A function to specify related entities to include in the query.</param>
        /// <param name="asNoTracking">True to retrieve entities without tracking; otherwise, false.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the list of entities.</returns>
        public async Task<List<T>> GetListAsync<T>(
            Func<IQueryable<T>, IIncludableQueryable<T, object>> includes,
            bool asNoTracking,
            CancellationToken cancellationToken = default) where T : BaseEntity
        {
            IQueryable<T> query = _dbContext.Set<T>();

            if (includes != null)
            {
                query = includes(query);
            }

            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            List<T> items = await query.ToListAsync(cancellationToken).ConfigureAwait(false);

            return items;
        }

        /// <summary>
        /// Asynchronously retrieves a filtered list of entities of the specified type from the database.
        /// </summary>
        /// <typeparam name="T">The type of entities to retrieve.</typeparam>
        /// <param name="condition">The condition to filter entities.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the filtered list of entities.</returns>
        public Task<List<T>> GetListAsync<T>(Expression<Func<T, bool>> condition, CancellationToken cancellationToken = default) where T : BaseEntity
        {
            return GetListAsync(condition, false, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a filtered list of entities of the specified type from the database, optionally without tracking.
        /// </summary>
        /// <typeparam name="T">The type of entities to retrieve.</typeparam>
        /// <param name="condition">The condition to filter entities.</param>
        /// <param name="asNoTracking">True to retrieve entities without tracking; otherwise, false.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the filtered list of entities.</returns>
        public Task<List<T>> GetListAsync<T>(
            Expression<Func<T, bool>> condition,
            bool asNoTracking,
            CancellationToken cancellationToken = default) where T : BaseEntity
        {
            return GetListAsync(condition, null, asNoTracking, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a list of entities of the specified type from the database using the provided specification.
        /// </summary>
        /// <typeparam name="T">The type of entities to retrieve.</typeparam>
        /// <param name="specification">The specification to apply to the query.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the list of entities.</returns>
        public async Task<List<T>> GetListAsync<T>(
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

            List<T> items = await query.ToListAsync(cancellationToken).ConfigureAwait(false);

            return items;
        }

        /// <summary>
        /// Asynchronously retrieves a list of entities of the specified type from the database using the provided specification, optionally without tracking.
        /// </summary>
        /// <typeparam name="T">The type of entities to retrieve.</typeparam>
        /// <param name="specification">The specification to apply to the query.</param>
        /// <param name="asNoTracking">True to retrieve entities without tracking; otherwise, false.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the list of entities.</returns>
        public Task<List<T>> GetListAsync<T>(Specification<T> specification, CancellationToken cancellationToken = default) where T : BaseEntity
        {
            return GetListAsync(specification, false, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a list of projected entities of the specified type from the database.
        /// </summary>
        /// <typeparam name="T">The type of entities to retrieve.</typeparam>
        /// <typeparam name="TProjectedType">The type of projected entities.</typeparam>
        /// <param name="selectExpression">The expression to select and project entities.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the list of projected entities.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the provided select expression is null.</exception>
        public async Task<List<T>> GetListAsync<T>(
            Specification<T> specification,
            bool asNoTracking,
            CancellationToken cancellationToken = default) where T : BaseEntity
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

            return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously retrieves a filtered list of projected entities of the specified type from the database.
        /// </summary>
        /// <typeparam name="T">The type of entities to retrieve.</typeparam>
        /// <typeparam name="TProjectedType">The type of projected entities.</typeparam>
        /// <param name="condition">The condition to filter entities.</param>
        /// <param name="selectExpression">The expression to select and project entities.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the filtered list of projected entities.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the provided select expression is null.</exception>
        public async Task<List<TProjectedType>> GetListAsync<T, TProjectedType>(
            Expression<Func<T, TProjectedType>> selectExpression,
            CancellationToken cancellationToken = default) where T : BaseEntity
        {
            if (selectExpression == null)
            {
                throw new ArgumentNullException(nameof(selectExpression));
            }

            List<TProjectedType> entities = await _dbContext.Set<T>()
                .Select(selectExpression).ToListAsync(cancellationToken).ConfigureAwait(false);

            return entities;
        }

        /// <summary>
        /// Asynchronously retrieves a list of projected entities of the specified type from the database using the provided specification.
        /// </summary>
        /// <typeparam name="T">The type of entities to retrieve.</typeparam>
        /// <typeparam name="TProjectedType">The type of projected entities.</typeparam>
        /// <param name="specification">The specification to apply to the query.</param>
        /// <param name="selectExpression">The expression to select and project entities.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the list of projected entities.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the provided select expression is null.</exception>
        public async Task<List<TProjectedType>> GetListAsync<T, TProjectedType>(
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

            List<TProjectedType> projectedEntites = await query.Select(selectExpression)
                .ToListAsync(cancellationToken).ConfigureAwait(false);

            return projectedEntites;
        }

        /// <summary>
        /// Asynchronously retrieves a paginated list of entities of the specified type from the database using the provided pagination specification.
        /// </summary>
        /// <typeparam name="T">The type of entities to retrieve.</typeparam>
        /// <param name="specification">The pagination specification to apply to the query.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the paginated list of entities.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the provided pagination specification is null.</exception>
        public async Task<List<TProjectedType>> GetListAsync<T, TProjectedType>(
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

            return await query.Select(selectExpression)
                .ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously retrieves a paginated list of projected entities of the specified type from the database using the provided pagination specification.
        /// </summary>
        /// <typeparam name="T">The type of entities to retrieve.</typeparam>
        /// <typeparam name="TProjectedType">The type of projected entities.</typeparam>
        /// <param name="specification">The pagination specification to apply to the query.</param>
        /// <param name="selectExpression">The expression to select and project entities.</param>
        /// <param name="cancellationToken">
        public async Task<PaginatedList<T>> GetListAsync<T>(
            PaginationSpecification<T> specification,
            CancellationToken cancellationToken = default) where T : BaseEntity
        {
            if (specification == null)
            {
                throw new ArgumentNullException(nameof(specification));
            }

            PaginatedList<T> paginatedList = await _dbContext.Set<T>().ToPaginatedListAsync(specification, cancellationToken);
            return paginatedList;
        }

        /// <summary>
        /// Asynchronously retrieves a paginated list of projected entities of the specified type from the database using the provided pagination specification.
        /// </summary>
        /// <typeparam name="T">The type of entities to retrieve.</typeparam>
        /// <typeparam name="TProjectedType">The type of projected entities.</typeparam>
        /// <param name="specification">The pagination specification to apply to the query.</param>
        /// <param name="selectExpression">The expression to select and project entities.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the paginated list of projected entities.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the provided pagination specification or select expression is null.</exception>
        public async Task<PaginatedList<TProjectedType>> GetListAsync<T, TProjectedType>(
            PaginationSpecification<T> specification,
            Expression<Func<T, TProjectedType>> selectExpression,
            CancellationToken cancellationToken = default) where T : BaseEntity
            where TProjectedType : class
        {
            if (specification == null)
            {
                throw new ArgumentNullException(nameof(specification));
            }

            if (selectExpression == null)
            {
                throw new ArgumentNullException(nameof(selectExpression));
            }

            IQueryable<T> query = _dbContext.Set<T>().GetSpecifiedQuery((SpecificationBase<T>)specification);

            PaginatedList<TProjectedType> paginatedList = await query.Select(selectExpression)
                .ToPaginatedListAsync(specification.PageIndex, specification.PageSize, cancellationToken);
            return paginatedList;
        }
    }
}