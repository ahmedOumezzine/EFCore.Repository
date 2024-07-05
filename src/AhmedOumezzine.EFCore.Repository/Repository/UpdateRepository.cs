using AhmedOumezzine.EFCore.Repository.Entities;
using AhmedOumezzine.EFCore.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Data;

namespace AhmedOumezzine.EFCore.Repository.Repository
{
    public sealed partial class Repository<TDbContext> : IRepository
        where TDbContext : DbContext
    {
        /// <summary>
        /// Updates the specified entity in the database.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity to update.</typeparam>
        /// <param name="entity">The entity to update.</param>
        /// <exception cref="ArgumentNullException">Thrown when the entity is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the entity type is not part of the DbContext model or when the primary key value of the entity is not valid.</exception>
        public void Update<TEntity>(TEntity entity) where TEntity : BaseEntity
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            EntityEntry<TEntity> trackedEntity = _dbContext.ChangeTracker
                                                           .Entries<TEntity>()
                                                           .FirstOrDefault(x => x.Entity == entity);

            if (trackedEntity == null)
            {
                IEntityType entityType = _dbContext.Model.FindEntityType(typeof(TEntity));

                if (entityType == null)
                {
                    throw new InvalidOperationException($"{typeof(TEntity).Name} is not part of EF Core DbContext model");
                }

                string primaryKeyName = entityType.FindPrimaryKey().Properties.Select(p => p.Name).FirstOrDefault();

                if (primaryKeyName != null)
                {
                    Type primaryKeyType = entityType.FindPrimaryKey().Properties.Select(p => p.ClrType).FirstOrDefault();

                    object primaryKeyDefaultValue = primaryKeyType.IsValueType ? Activator.CreateInstance(primaryKeyType) : null;

                    object primaryValue = entity.GetType().GetProperty(primaryKeyName).GetValue(entity, null);

                    if (primaryKeyDefaultValue.Equals(primaryValue))
                    {
                        throw new InvalidOperationException("The primary key value of the entity to be updated is not valid.");
                    }
                }

                _dbContext.Set<TEntity>().Update(entity);
            }
        }

        /// <summary>
        /// Updates a collection of entities in the database.
        /// </summary>
        /// <typeparam name="TEntity">The type of entities to update.</typeparam>
        /// <param name="entities">The collection of entities to update.</param>
        /// <exception cref="ArgumentNullException">Thrown when the collection of entities is null.</exception>
        public void Update<TEntity>(IEnumerable<TEntity> entities) where TEntity : BaseEntity
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            _dbContext.Set<TEntity>().UpdateRange(entities);
        }

        /// <summary>
        /// Asynchronously updates the specified entity in the database.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity to update.</typeparam>
        /// <param name="entity">The entity to update.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result represents the number of entities updated in the database.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the entity is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the entity type is not part of the DbContext model or when the primary key value of the entity is not valid.</exception>
        public async Task<int> UpdateAsync<TEntity>(TEntity entity,
                                                    CancellationToken cancellationToken = default) where TEntity : BaseEntity
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            EntityEntry<TEntity> trackedEntity = _dbContext.ChangeTracker.Entries<TEntity>().FirstOrDefault(x => x.Entity == entity);

            if (trackedEntity == null)
            {
                IEntityType entityType = _dbContext.Model.FindEntityType(typeof(TEntity));

                if (entityType == null)
                {
                    throw new InvalidOperationException($"{typeof(TEntity).Name} is not part of EF Core DbContext model");
                }

                string primaryKeyName = entityType.FindPrimaryKey().Properties.Select(p => p.Name).FirstOrDefault();

                if (primaryKeyName != null)
                {
                    Type primaryKeyType = entityType.FindPrimaryKey().Properties.Select(p => p.ClrType).FirstOrDefault();

                    object primaryKeyDefaultValue = primaryKeyType.IsValueType ? Activator.CreateInstance(primaryKeyType) : null;

                    object primaryValue = entity.GetType().GetProperty(primaryKeyName).GetValue(entity, null);

                    if (primaryKeyDefaultValue.Equals(primaryValue))
                    {
                        throw new InvalidOperationException("The primary key value of the entity to be updated is not valid.");
                    }
                }

                _dbContext.Set<TEntity>().Update(entity);
            }

            int count = await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return count;
        }

        /// <summary>
        /// Asynchronously updates a collection of entities in the database.
        /// </summary>
        /// <typeparam name="T">The type of entities to update.</typeparam>
        /// <param name="entities">The collection of entities to update.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result represents the number of entities updated in the database.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the collection of entities is null.</exception>
        public async Task<int> UpdateAsync<T>(IEnumerable<T> entities,
                                             CancellationToken cancellationToken = default) where T : BaseEntity
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            _dbContext.Set<T>().UpdateRange(entities);
            int count = await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return count;
        }
    }
}