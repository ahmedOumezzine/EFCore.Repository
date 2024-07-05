using AhmedOumezzine.EFCore.Repository.Entities;
using AhmedOumezzine.EFCore.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using System.Data;
using System.Globalization;
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
        public Task<T> GetByIdAsync<T>(object id, CancellationToken cancellationToken = default) where T : BaseEntity
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            return GetByIdAsync<T>(id, false, cancellationToken);
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
        public Task<T> GetByIdAsync<T>(object id, bool asNoTracking, CancellationToken cancellationToken = default) where T : BaseEntity
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            return GetByIdAsync<T>(id, null, asNoTracking, cancellationToken);
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
        public Task<T> GetByIdAsync<T>(
            object id,
            Func<IQueryable<T>, IIncludableQueryable<T, object>> includes,
            CancellationToken cancellationToken = default) where T : BaseEntity
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            return GetByIdAsync(id, includes, false, cancellationToken);
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

        public async Task<T> GetByIdAsync<T>(
            object id,
            Func<IQueryable<T>, IIncludableQueryable<T, object>> includes,
            bool asNoTracking = false,
            CancellationToken cancellationToken = default) where T : BaseEntity
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            IEntityType entityType = _dbContext.Model.FindEntityType(typeof(T));

            string primaryKeyName = entityType.FindPrimaryKey().Properties.Select(p => p.Name).FirstOrDefault();
            Type primaryKeyType = entityType.FindPrimaryKey().Properties.Select(p => p.ClrType).FirstOrDefault();

            if (primaryKeyName == null || primaryKeyType == null)
            {
                throw new ArgumentException("Entity does not have any primary key defined", nameof(id));
            }

            object primaryKeyValue = null;

            try
            {
                primaryKeyValue = Convert.ChangeType(id, primaryKeyType, CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                throw new ArgumentException($"You can not assign a value of type {id.GetType()} to a property of type {primaryKeyType}");
            }

            ParameterExpression pe = Expression.Parameter(typeof(T), "entity");
            MemberExpression me = Expression.Property(pe, primaryKeyName);
            ConstantExpression constant = Expression.Constant(primaryKeyValue, primaryKeyType);
            BinaryExpression body = Expression.Equal(me, constant);
            Expression<Func<T, bool>> expressionTree = Expression.Lambda<Func<T, bool>>(body, new[] { pe });

            IQueryable<T> query = _dbContext.Set<T>();

            if (includes != null)
            {
                query = includes(query);
            }

            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            T entity = await query.FirstOrDefaultAsync(expressionTree, cancellationToken).ConfigureAwait(false);
            return entity;
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

        public async Task<TProjectedType> GetByIdAsync<T, TProjectedType>(
            object id,
            Expression<Func<T, TProjectedType>> selectExpression,
            CancellationToken cancellationToken = default) where T : BaseEntity
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (selectExpression == null)
            {
                throw new ArgumentNullException(nameof(selectExpression));
            }

            IEntityType entityType = _dbContext.Model.FindEntityType(typeof(T));

            string primaryKeyName = entityType.FindPrimaryKey().Properties.Select(p => p.Name).FirstOrDefault();
            Type primaryKeyType = entityType.FindPrimaryKey().Properties.Select(p => p.ClrType).FirstOrDefault();

            if (primaryKeyName == null || primaryKeyType == null)
            {
                throw new ArgumentException("Entity does not have any primary key defined", nameof(id));
            }

            object primaryKeyValue = null;

            try
            {
                primaryKeyValue = Convert.ChangeType(id, primaryKeyType, CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                throw new ArgumentException($"You can not assign a value of type {id.GetType()} to a property of type {primaryKeyType}");
            }

            ParameterExpression pe = Expression.Parameter(typeof(T), "entity");
            MemberExpression me = Expression.Property(pe, primaryKeyName);
            ConstantExpression constant = Expression.Constant(primaryKeyValue, primaryKeyType);
            BinaryExpression body = Expression.Equal(me, constant);
            Expression<Func<T, bool>> expressionTree = Expression.Lambda<Func<T, bool>>(body, new[] { pe });

            IQueryable<T> query = _dbContext.Set<T>();

            return await query.Where(expressionTree)
                              .Select(selectExpression)
                              .FirstOrDefaultAsync(cancellationToken)
                              .ConfigureAwait(false);
        }
    }
}