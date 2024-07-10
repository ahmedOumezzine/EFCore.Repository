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

        public Task<List<TEntity>> GetListAsync<TEntity>(CancellationToken cancellationToken = default)
                             where TEntity : BaseEntity
        {
            return GetListAsync<TEntity>(false, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a list of entities of the specified type from the database, optionally without tracking.
        /// </summary>
        /// <typeparam name="T">The type of entities to retrieve.</typeparam>
        /// <param name="asNoTracking">True to retrieve entities without tracking; otherwise, false.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the list of entities.</returns>
        public Task<List<TEntity>> GetListAsync<TEntity>(bool asNoTracking,
                                                         CancellationToken cancellationToken = default)
                                   where TEntity : BaseEntity
        {
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> nullValue = null;
            return GetListAsync(nullValue, asNoTracking, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves a list of entities of the specified type from the database, optionally including related entities.
        /// </summary>
        /// <typeparam name="T">The type of entities to retrieve.</typeparam>
        /// <param name="includes">A function to specify related entities to include in the query.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the list of entities.</returns>
        public Task<List<TEntity>> GetListAsync<TEntity>(
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> includes,
            CancellationToken cancellationToken = default) where TEntity : BaseEntity
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
        public async Task<List<TEntity>> GetListAsync<TEntity>(
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> includes,
            bool asNoTracking,
            CancellationToken cancellationToken = default) where TEntity : BaseEntity
        {
            IQueryable<TEntity> query = _dbContext.Set<TEntity>();

            if (includes != null)
            {
                query = includes(query);
            }

            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            List<TEntity> items = await query.ToListAsync(cancellationToken).ConfigureAwait(false);

            return items;
        }

        /// <summary>
        /// Asynchronously retrieves a filtered list of entities of the specified type from the database.
        /// </summary>
        /// <typeparam name="T">The type of entities to retrieve.</typeparam>
        /// <param name="condition">The condition to filter entities.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the filtered list of entities.</returns>
        public Task<List<TEntity>> GetListAsync<TEntity>(Expression<Func<TEntity, bool>> condition,
                                                         CancellationToken cancellationToken = default)
                                   where TEntity : BaseEntity
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
        public Task<List<TEntity>> GetListAsync<TEntity>(
            Expression<Func<TEntity, bool>> condition,
            bool asNoTracking,
            CancellationToken cancellationToken = default) where TEntity : BaseEntity
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
        public async Task<List<TEntity>> GetListAsync<TEntity>(
            Expression<Func<TEntity, bool>> condition,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> includes,
            bool asNoTracking,
            CancellationToken cancellationToken = default) where TEntity : BaseEntity
        {
            IQueryable<TEntity> query = _dbContext.Set<TEntity>();

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

            List<TEntity> items = await query.ToListAsync(cancellationToken).ConfigureAwait(false);

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
        public Task<List<TEntity>> GetListAsync<TEntity>(Specification<TEntity> specification, CancellationToken cancellationToken = default) where TEntity : BaseEntity
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
        public async Task<List<TEntity>> GetListAsync<TEntity>(
            Specification<TEntity> specification,
            bool asNoTracking,
            CancellationToken cancellationToken = default) where TEntity : BaseEntity
        {
            IQueryable<TEntity> query = _dbContext.Set<TEntity>();

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
        public async Task<List<TProjectedType>> GetListAsync<TEntity, TProjectedType>(
            Expression<Func<TEntity, TProjectedType>> selectExpression,
            CancellationToken cancellationToken = default) where TEntity : BaseEntity
        {
            if (selectExpression == null)
            {
                throw new ArgumentNullException(nameof(selectExpression));
            }

            List<TProjectedType> entities = await _dbContext.Set<TEntity>()
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
        public async Task<List<TProjectedType>> GetListAsync<TEntity, TProjectedType>(
            Expression<Func<TEntity, bool>> condition,
            Expression<Func<TEntity, TProjectedType>> selectExpression,
            CancellationToken cancellationToken = default) where TEntity : BaseEntity
        {
            if (selectExpression == null)
            {
                throw new ArgumentNullException(nameof(selectExpression));
            }

            IQueryable<TEntity> query = _dbContext.Set<TEntity>();

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
        public async Task<List<TProjectedType>> GetListAsync<TEntity, TProjectedType>(
            Specification<TEntity> specification,
            Expression<Func<TEntity, TProjectedType>> selectExpression,
            CancellationToken cancellationToken = default) where TEntity : BaseEntity
        {
            if (selectExpression == null)
            {
                throw new ArgumentNullException(nameof(selectExpression));
            }

            IQueryable<TEntity> query = _dbContext.Set<TEntity>();

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
        public async Task<PaginatedList<TEntity>> GetListAsync<TEntity>(
            PaginationSpecification<TEntity> specification,
            CancellationToken cancellationToken = default) where TEntity : BaseEntity
        {
            if (specification == null)
            {
                throw new ArgumentNullException(nameof(specification));
            }

            PaginatedList<TEntity> paginatedList = await _dbContext.Set<TEntity>().ToPaginatedListAsync(specification, cancellationToken);
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
        public async Task<PaginatedList<TProjectedType>> GetListAsync<TEntity, TProjectedType>(
            PaginationSpecification<TEntity> specification,
            Expression<Func<TEntity, TProjectedType>> selectExpression,
            CancellationToken cancellationToken = default) where TEntity : BaseEntity
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

            IQueryable<TEntity> query = _dbContext.Set<TEntity>().GetSpecifiedQuery((SpecificationBase<TEntity>)specification);

            PaginatedList<TProjectedType> paginatedList = await query.Select(selectExpression)
                .ToPaginatedListAsync(specification.PageIndex, specification.PageSize, cancellationToken);
            return paginatedList;
        }
    }
}