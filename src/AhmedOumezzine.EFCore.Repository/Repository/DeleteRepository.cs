﻿using AhmedOumezzine.EFCore.Repository.Entities;
using AhmedOumezzine.EFCore.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace AhmedOumezzine.EFCore.Repository.Repository
{
    public sealed partial class Repository<TDbContext> : IRepository
        where TDbContext : DbContext
    {
        /// <summary>
        /// Removes an entity from the database context.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity to be removed.</typeparam>
        /// <param name="entity">The entity to remove from the context.</param>
        /// <exception cref="ArgumentNullException">Thrown when the provided entity is null.</exception>

        public void Remove<TEntity>(TEntity entity)
                    where TEntity : BaseEntity
        {
            CheckEntityIsNull<TEntity>(entity);
            SetDeleted(entity);
            _dbContext.Set<TEntity>().Remove(entity);
        }

        /// <summary>
        /// Removes a collection of entities from the database context.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entities to be removed.</typeparam>
        /// <param name="entities">The collection of entities to remove from the context.</param>
        /// <exception cref="ArgumentNullException">Thrown when the provided collection of entities is null.</exception>

        public void Remove<TEntity>(IEnumerable<TEntity> entities)
                    where TEntity : BaseEntity
        {
            CheckEntitiesIsNull<TEntity>(entities);
            SetDeleted(entities);
            _dbContext.Set<TEntity>().RemoveRange(entities);
        }

        /// <summary>
        /// Asynchronously removes an entity from the database context and saves the changes.
        /// </summary>
        /// <typeparam name="T">The type of the entity to be removed.</typeparam>
        /// <param name="entity">The entity to remove from the context.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>The number of state entries written to the database.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the provided entity is null.</exception>
        public async Task<int> DeleteAsync<TEntity>(TEntity entity,
                                              CancellationToken cancellationToken = default)
                               where TEntity : BaseEntity
        {
            CheckEntityIsNull<TEntity>(entity);
            SetDeleted(entity);
            _dbContext.Set<TEntity>().Remove(entity);
            int count = await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return count;
        }

        /// <summary>
        /// Asynchronously removes a collection of entities from the database context and saves the changes.
        /// </summary>
        /// <typeparam name="T">The type of the entities to be removed.</typeparam>
        /// <param name="entities">The collection of entities to remove from the context.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>The number of state entries written to the database.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the provided collection of entities is null.</exception>

        public async Task<int> DeleteAsync<TEntity>(IEnumerable<TEntity> entities,
                                              CancellationToken cancellationToken = default)
                               where TEntity : BaseEntity
        {
            CheckEntitiesIsNull<TEntity>(entities);
            SetDeleted(entities);
            _dbContext.Set<TEntity>().RemoveRange(entities);
            int count = await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return count;
        }
    }
}