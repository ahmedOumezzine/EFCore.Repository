using AhmedOumezzine.EFCore.Repository.Entities;
using AhmedOumezzine.EFCore.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AhmedOumezzine.EFCore.Repository.Repository
{
    public sealed partial class Repository<TDbContext> : IRepository
        where TDbContext : DbContext
    {
        /// <summary>
        /// Asynchronously checks if any entity of the specified type exists in the database context.
        /// </summary>
        /// <typeparam name="T">The type of the entity to check for existence.</typeparam>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean value that indicates whether any entity of the specified type exists in the context.</returns>

        public Task<bool> ExistsAsync<TEntity>(CancellationToken cancellationToken = default)
                          where TEntity : BaseEntity
        {
            return ExistsAsync<TEntity>(null, cancellationToken);
        }

        /// <summary>
        /// Asynchronously checks if any entity of the specified type that matches the given condition exists in the database context.
        /// </summary>
        /// <typeparam name="T">The type of the entity to check for existence.</typeparam>
        /// <param name="condition">An optional expression representing the condition to check for.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean value that indicates whether any entity of the specified type that matches the condition exists in the context.</returns>

        public async Task<bool> ExistsAsync<TEntity>(Expression<Func<TEntity, bool>> condition,
                                                     CancellationToken cancellationToken = default)
                                where TEntity : BaseEntity
        {
            IQueryable<TEntity> query = _dbContext.Set<TEntity>();

            if (condition == null)
            {
                return await query.AnyAsync(cancellationToken);
            }

            bool isExists = await query.AnyAsync(condition, cancellationToken).ConfigureAwait(false);
            return isExists;
        }

        /// <summary>
        /// Asynchronously checks if any entity of the specified type with the given primary key value exists in the database context.
        /// </summary>
        /// <typeparam name="T">The type of the entity to check for existence.</typeparam>
        /// <param name="id">The primary key value to check for.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean value that indicates whether any entity of the specified type with the given primary key value exists in the context.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the provided primary key value is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the entity does not have any primary key defined or the provided primary key value cannot be assigned to the primary key property.</exception>

        public async Task<bool> ExistsByIdAsync<TEntity>(Guid? id,
                                                   CancellationToken cancellationToken = default)
                                where TEntity : BaseEntity
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }
            Expression<Func<TEntity, bool>> expressionTree = x => x.Id == id && x.IsDeleted == false;

            IQueryable<TEntity> query = _dbContext.Set<TEntity>();

            bool isExistent = await query.AnyAsync(expressionTree, cancellationToken).ConfigureAwait(false);
            return isExistent;
        }
    }
}