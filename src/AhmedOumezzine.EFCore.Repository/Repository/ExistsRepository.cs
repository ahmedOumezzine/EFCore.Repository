using AhmedOumezzine.EFCore.Repository.Entities;
using AhmedOumezzine.EFCore.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Data;
using System.Globalization;
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

        public Task<bool> ExistsAsync<T>(CancellationToken cancellationToken = default) where T : BaseEntity
        {
            return ExistsAsync<T>(null, cancellationToken);
        }

        /// <summary>
        /// Asynchronously checks if any entity of the specified type that matches the given condition exists in the database context.
        /// </summary>
        /// <typeparam name="T">The type of the entity to check for existence.</typeparam>
        /// <param name="condition">An optional expression representing the condition to check for.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean value that indicates whether any entity of the specified type that matches the condition exists in the context.</returns>

        public async Task<bool> ExistsAsync<T>(Expression<Func<T, bool>> condition, CancellationToken cancellationToken = default) where T : BaseEntity
        {
            IQueryable<T> query = _dbContext.Set<T>();

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

        public async Task<bool> ExistsByIdAsync<T>(object id, CancellationToken cancellationToken = default) where T : BaseEntity
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

            bool isExistent = await query.AnyAsync(expressionTree, cancellationToken).ConfigureAwait(false);
            return isExistent;
        }
    }
}