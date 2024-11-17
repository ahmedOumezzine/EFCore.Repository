using AhmedOumezzine.EFCore.Repository.Entities;
using AhmedOumezzine.EFCore.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Data;

namespace AhmedOumezzine.EFCore.Repository.Repository
{
    public sealed partial class Repository<TDbContext> : IRepository
        where TDbContext : DbContext
    {
        /// <summary>
        /// Adds an entity of the specified type to the database context.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity to add.</typeparam>
        /// <param name="entity">The entity to add. Cannot be null.</param>
        /// <returns>Void (no explicit return value).</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="entity"/> parameter is null.</exception>
        public void Add<TEntity>(TEntity entity)
                    where TEntity : BaseEntity
        {
            CheckEntityIsNull<TEntity>(entity);
            SetCreatedOnUtc(entity);
            _dbContext.Set<TEntity>().Add(entity);
        }

        /// <summary>
        /// Asynchronously adds an entity of the specified type to the database context.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity to add.</typeparam>
        /// <param name="entity">The entity to add. Cannot be null.</param>
        /// <param name="cancellationToken">Optional. The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="entity"/> parameter is null.</exception>
        public async Task AddAsync<TEntity>(TEntity entity,
                                           CancellationToken cancellationToken = default)
                          where TEntity : BaseEntity
        {
            CheckEntityIsNull<TEntity>(entity);
            SetCreatedOnUtc(entity);
            await _dbContext.Set<TEntity>().AddAsync(entity, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Adds a collection of entities of the specified type to the database context.
        /// </summary>
        /// <typeparam name="TEntity">The type of entities to add.</typeparam>
        /// <param name="entities">The collection of entities to add. Cannot be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="entities"/> parameter is null.</exception>
        public void Add<TEntity>(IEnumerable<TEntity> entities)
                    where TEntity : BaseEntity
        {
            CheckEntitiesIsNull<TEntity>(entities);
            SetCreatedOnUtc(entities);
            _dbContext.Set<TEntity>().AddRange(entities);
        }

        /// <summary>
        /// Asynchronously adds a collection of entities of the specified type to the database context.
        /// </summary>
        /// <typeparam name="TEntity">The type of entities to add.</typeparam>
        /// <param name="entities">The collection of entities to add. Cannot be null.</param>
        /// <param name="cancellationToken">Optional. The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="entities"/> parameter is null.</exception>
        public async Task AddAsync<TEntity>(IEnumerable<TEntity> entities,
                                            CancellationToken cancellationToken = default)
                          where TEntity : BaseEntity
        {
            CheckEntitiesIsNull<TEntity>(entities);
            SetCreatedOnUtc(entities);
            await _dbContext.Set<TEntity>().AddRangeAsync(entities, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously inserts an entity of the specified type into the database context and returns its primary key values.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity to insert.</typeparam>
        /// <param name="entity">The entity to insert. Cannot be null.</param>
        /// <param name="cancellationToken">Optional. The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>An array of primary key values of the inserted entity.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="entity"/> parameter is null.</exception>
        public async Task<object[]> InsertAsync<TEntity>(TEntity entity,
                                                         CancellationToken cancellationToken = default)
                                    where TEntity : BaseEntity
        {
            CheckEntityIsNull<TEntity>(entity);
            SetCreatedOnUtc(entity);
            EntityEntry<TEntity> entityEntry = await _dbContext.Set<TEntity>().AddAsync(entity, cancellationToken).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            object[] primaryKeyValue = entityEntry.Metadata.FindPrimaryKey().Properties.Select(p => entityEntry.Property(p.Name).CurrentValue).ToArray();

            return primaryKeyValue;
        }

        /// <summary>
        /// Asynchronously inserts a collection of entities of the specified type into the database context.
        /// </summary>
        /// <typeparam name="TEntity">The type of entities to insert.</typeparam>
        /// <param name="entities">The collection of entities to insert. Cannot be null.</param>
        /// <param name="cancellationToken">Optional. The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="entities"/> parameter is null.</exception>
        public async Task InsertAsync<TEntity>(IEnumerable<TEntity> entities,
                                               CancellationToken cancellationToken = default)
                          where TEntity : BaseEntity
        {
            CheckEntitiesIsNull<TEntity>(entities);
            SetCreatedOnUtc(entities);
            await _dbContext.Set<TEntity>().AddRangeAsync(entities, cancellationToken).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}