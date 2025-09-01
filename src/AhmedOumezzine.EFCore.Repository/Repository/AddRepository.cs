using AhmedOumezzine.EFCore.Repository.Entities;
using AhmedOumezzine.EFCore.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AhmedOumezzine.EFCore.Repository.Repository
{
    /// <summary>
    /// Partial implementation of the generic repository for adding entities.
    /// Provides methods to add, insert, and conditionally persist entities.
    /// </summary>
    /// <typeparam name="TDbContext">The type of the database context.</typeparam>
    public sealed partial class Repository<TDbContext> : IRepository
        where TDbContext : DbContext
    {

        #region GetByIdAsync - Projection

        /// <summary>
        /// Retrieves a projected entity by its primary key.
        /// </summary>
        public async Task<TProjectedType> GetByIdAsync<TEntity, TProjectedType>(
            Guid? id,
            Expression<Func<TEntity, TProjectedType>> selectExpression,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
            where TProjectedType : class
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (selectExpression == null) throw new ArgumentNullException(nameof(selectExpression));

            var entity = await _dbContext.Set<TEntity>()
                .Where(e => e.Id == id && !e.IsDeleted)
                .Select(selectExpression)
                .FirstOrDefaultAsync(cancellationToken);

            return entity;
        }

        #endregion

        #region GetAsync - Projection

        /// <summary>
        /// Retrieves a projected entity based on a condition.
        /// </summary>
        public async Task<TProjectedType> GetAsync<TEntity, TProjectedType>(
            Expression<Func<TEntity, bool>> condition,
            Expression<Func<TEntity, TProjectedType>> selectExpression,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
            where TProjectedType : class
        {
            if (condition == null) throw new ArgumentNullException(nameof(condition));
            if (selectExpression == null) throw new ArgumentNullException(nameof(selectExpression));

            var entity = await _dbContext.Set<TEntity>()
                .Where(e => !e.IsDeleted)
                .Where(condition)
                .Select(selectExpression)
                .FirstOrDefaultAsync(cancellationToken);

            return entity;
        }

        #endregion
        #region Add (To Context - No Save)

        /// <summary>
        /// Adds an entity to the context. Does NOT save changes to the database.
        /// </summary>
        /// <typeparam name="TEntity">The entity type, must inherit from <see cref="BaseEntity"/>.</typeparam>
        /// <param name="entity">The entity to add. Cannot be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="entity"/> is null.</exception>
        public void Add<TEntity>(TEntity entity) where TEntity : BaseEntity
        {
            CheckEntityIsNull(entity);
            SetCreatedOnUtc(entity);
            _dbContext.Set<TEntity>().Add(entity);
        }

        /// <summary>
        /// Asynchronously adds an entity to the context. Does NOT save changes to the database.
        /// </summary>
        /// <typeparam name="TEntity">The entity type, must inherit from <see cref="BaseEntity"/>.</typeparam>
        /// <param name="entity">The entity to add. Cannot be null.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="entity"/> is null.</exception>
        public async Task AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            CheckEntityIsNull(entity);
            SetCreatedOnUtc(entity);
            await _dbContext.Set<TEntity>().AddAsync(entity, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Adds a collection of entities to the context. Does NOT save changes.
        /// </summary>
        /// <typeparam name="TEntity">The entity type, must inherit from <see cref="BaseEntity"/>.</typeparam>
        /// <param name="entities">The collection of entities to add. Cannot be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="entities"/> is null.</exception>
        public void Add<TEntity>(IEnumerable<TEntity> entities) where TEntity : BaseEntity
        {
            CheckEntitiesIsNull(entities);
            SetCreatedOnUtc(entities);
            _dbContext.Set<TEntity>().AddRange(entities);
        }

        /// <summary>
        /// Asynchronously adds a collection of entities to the context. Does NOT save changes.
        /// </summary>
        /// <typeparam name="TEntity">The entity type, must inherit from <see cref="BaseEntity"/>.</typeparam>
        /// <param name="entities">The collection of entities to add. Cannot be null.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="entities"/> is null.</exception>
        public async Task AddAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            CheckEntitiesIsNull(entities);
            SetCreatedOnUtc(entities);
            await _dbContext.Set<TEntity>().AddRangeAsync(entities, cancellationToken).ConfigureAwait(false);
        }

        #endregion

        #region Add and Save (Immediate Persistence)

        /// <summary>
        /// Adds an entity and saves changes. Returns the primary key values of the inserted entity.
        /// </summary>
        /// <typeparam name="TEntity">The entity type, must inherit from <see cref="BaseEntity"/>.</typeparam>
        /// <param name="entity">The entity to insert. Cannot be null.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>An array of primary key values.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="entity"/> is null.</exception>
        public async Task<object[]> AddAndSaveAsync<TEntity>(
            TEntity entity,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            CheckEntityIsNull(entity);
            SetCreatedOnUtc(entity);

            await _dbContext.Set<TEntity>().AddAsync(entity, cancellationToken).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            var entry = _dbContext.Entry(entity);
            var primaryKey = entry.Metadata.FindPrimaryKey();
            return primaryKey.Properties
                .Select(p => entry.Property(p.Name).CurrentValue)
                .ToArray();
        }

        /// <summary>
        /// Adds a collection of entities and saves changes immediately.
        /// </summary>
        /// <typeparam name="TEntity">The entity type, must inherit from <see cref="BaseEntity"/>.</typeparam>
        /// <param name="entities">The collection of entities to insert. Cannot be null.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="entities"/> is null.</exception>
        public async Task AddRangeAndSaveAsync<TEntity>(
            IEnumerable<TEntity> entities,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            CheckEntitiesIsNull(entities);
            SetCreatedOnUtc(entities);

            await _dbContext.Set<TEntity>().AddRangeAsync(entities, cancellationToken).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        #endregion

        #region Conditional Add

        /// <summary>
        /// Adds an entity only if no existing entity matches the specified predicate.
        /// </summary>
        /// <typeparam name="TEntity">The entity type, must inherit from <see cref="BaseEntity"/>.</typeparam>
        /// <param name="predicate">The condition to check for existence.</param>
        /// <param name="entity">The entity to add if not exists.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>True if added; false if an entity already exists.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="entity"/> or <paramref name="predicate"/> is null.</exception>
        public async Task<bool> AddIfNotExistsAsync<TEntity>(
            Expression<Func<TEntity, bool>> predicate,
            TEntity entity,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            CheckEntityIsNull(entity);

            var exists = await _dbContext.Set<TEntity>().AnyAsync(predicate, cancellationToken).ConfigureAwait(false);
            if (exists) return false;

            SetCreatedOnUtc(entity);
            await _dbContext.Set<TEntity>().AddAsync(entity, cancellationToken).ConfigureAwait(false);
            return true;
        }

        /// <summary>
        /// Attempts to add an entity and returns success status without throwing.
        /// Useful for bulk operations where duplicates are expected.
        /// </summary>
        /// <typeparam name="TEntity">The entity type, must inherit from <see cref="BaseEntity"/>.</typeparam>
        /// <param name="entity">The entity to add.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>True if added successfully; false if a database error occurred (e.g., duplicate key).</returns>
        public async Task<bool> TryAddAsync<TEntity>(
            TEntity entity,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            try
            {
                await AddAsync(entity, cancellationToken).ConfigureAwait(false);
                return true;
            }
            catch (DbUpdateException)
            {
                return false;
            }
        }

        #endregion
         
    }
}