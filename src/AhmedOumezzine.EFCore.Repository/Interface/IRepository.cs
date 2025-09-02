using AhmedOumezzine.EFCore.Repository.Entities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;
using System.Linq.Expressions;

namespace AhmedOumezzine.EFCore.Repository.Interface
{
    /// <summary>
    /// Generic repository interface for CRUD operations, transactions, and advanced querying.
    /// Extends <see cref="IQueryRepository"/> for comprehensive data access capabilities.
    /// </summary>
    public interface IRepository : IQueryRepository
    {
        #region Transactions

        /// <summary>
        /// Begins a new transaction with the specified isolation level.
        /// </summary>
        /// <param name="isolationLevel">The isolation level (default: Unspecified).</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>A task representing the transaction.</returns>
        Task<IDbContextTransaction> BeginTransactionAsync(
            IsolationLevel isolationLevel = IsolationLevel.Unspecified,
            CancellationToken cancellationToken = default);

        #endregion Transactions

        #region Add (Add to context, no save)

        /// <summary>
        /// Adds an entity to the context. Does NOT save changes.
        /// </summary>
        void Add<TEntity>(TEntity entity) where TEntity : BaseEntity;

        /// <summary>
        /// Asynchronously adds an entity to the context. Does NOT save changes.
        /// </summary>
        Task AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        /// <summary>
        /// Adds a collection of entities to the context. Does NOT save changes.
        /// </summary>
        void Add<TEntity>(IEnumerable<TEntity> entities) where TEntity : BaseEntity;

        /// <summary>
        /// Asynchronously adds a collection of entities to the context. Does NOT save changes.
        /// </summary>
        Task AddAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        #endregion Add (Add to context, no save)

        #region Add and Save (Immediate persistence)

        /// <summary>
        /// Adds an entity and saves changes. Returns the primary key values.
        /// </summary>
        /// <typeparam name="TEntity">Entity type deriving from <see cref="BaseEntity"/>.</typeparam>
        /// <param name="entity">The entity to insert.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>An array of primary key values.</returns>
        Task<object[]> AddAndSaveAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        /// <summary>
        /// Adds a collection of entities and saves changes immediately.
        /// </summary>
        /// <typeparam name="TEntity">Entity type deriving from <see cref="BaseEntity"/>.</typeparam>
        /// <param name="entities">The collection to insert.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>A task representing the operation.</returns>
        Task AddRangeAndSaveAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        #endregion Add and Save (Immediate persistence)

        #region Update

        /// <summary>
        /// Marks an entity as modified.
        /// </summary>
        void Update<TEntity>(TEntity entity) where TEntity : BaseEntity;

        /// <summary>
        /// Marks a collection of entities as modified.
        /// </summary>
        void Update<TEntity>(IEnumerable<TEntity> entities) where TEntity : BaseEntity;

        /// <summary>
        /// Marks an entity as modified and returns the number of affected rows after saving.
        /// </summary>
        Task<int> UpdateAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        /// <summary>
        /// Marks a collection of entities as modified and returns the number of affected rows after saving.
        /// </summary>
        Task<int> UpdateAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        #endregion Update

        #region Delete

        /// <summary>
        /// Marks an entity for deletion.
        /// </summary>
        void Remove<TEntity>(TEntity entity) where TEntity : BaseEntity;

        /// <summary>
        /// Marks a collection of entities for deletion.
        /// </summary>
        void Remove<TEntity>(IEnumerable<TEntity> entities) where TEntity : BaseEntity;

        /// <summary>
        /// Marks an entity for deletion and returns the number of affected rows after saving.
        /// </summary>
        Task<int> DeleteAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        /// <summary>
        /// Marks a collection of entities for deletion and returns the number of affected rows after saving.
        /// </summary>
        Task<int> DeleteAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        #endregion Delete

        #region Save

        /// <summary>
        /// Saves all changes made in this context to the database.
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The number of affected rows.</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        #endregion Save

        #region Raw SQL Commands (Modern & Safe)

        /// <summary>
        /// Executes a raw SQL command (INSERT, UPDATE, DELETE) and returns the number of affected rows.
        /// </summary>
        /// <param name="sql">The SQL command to execute.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The number of affected rows.</returns>
        Task<int> ExecuteSqlCommandAsync(
            string sql,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes a parameterized raw SQL command.
        /// </summary>
        /// <param name="sql">The SQL command with parameters (e.g., 'UPDATE Users SET Name = {0}').</param>
        /// <param name="parameters">The parameters to pass into the command.</param>
        /// <returns>The number of affected rows.</returns>
        Task<int> ExecuteSqlCommandAsync(
            string sql,
            params object[] parameters);

        /// <summary>
        /// Executes a parameterized raw SQL command with cancellation support.
        /// </summary>
        /// <param name="sql">The SQL command with parameters.</param>
        /// <param name="parameters">The parameters to pass into the command.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The number of affected rows.</returns>
        Task<int> ExecuteSqlCommandAsync(
            string sql,
            IEnumerable<object> parameters,
            CancellationToken cancellationToken = default);

        #endregion Raw SQL Commands (Modern & Safe)

        #region Scalar & Single SQL Results

        /// <summary>
        /// Executes a SQL query and returns a single scalar value (e.g., COUNT, MAX).
        /// </summary>
        /// <typeparam name="T">The type of the scalar result.</typeparam>
        /// <param name="sql">The SQL query (e.g., 'SELECT COUNT(*) FROM Users').</param>
        /// <param name="parameters">Optional parameters.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The scalar value, or default(T) if no result.</returns>
        Task<T> ExecuteScalarAsync<T>(
            string sql,
            IEnumerable<object> parameters = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes a raw SQL query and returns the first entity or null.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="sql">The SQL query.</param>
        /// <param name="parameters">Optional parameters.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The first entity or null.</returns>
        Task<T> GetSingleFromSqlAsync<T>(
            string sql,
            IEnumerable<object> parameters = null,
            CancellationToken cancellationToken = default)
            where T : class;

        /// <summary>
        /// Checks if any row matches the given SQL query.
        /// </summary>
        /// <param name="sql">The SQL query that returns a count.</param>
        /// <param name="parameters">Optional parameters.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>True if at least one row matches; otherwise, false.</returns>
        Task<bool> ExistsBySqlAsync(
            string sql,
            IEnumerable<object> parameters = null,
            CancellationToken cancellationToken = default);

        #endregion Scalar & Single SQL Results

        #region Bulk Operations (EF Core 7+)

        /// <summary>
        /// Updates entities matching a condition without loading them into memory (EF Core 7+).
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="predicate">The condition to match (e.g., x => x.Status == "Pending").</param>
        /// <param name="updateAction">The update operation (e.g., x => x.SetProperty(u => u.Status, "Processed")).</param>
        /// <returns>The number of affected rows.</returns>
        Task<int> UpdateFromQueryAsync<TEntity>(
            Expression<Func<TEntity, bool>> predicate,
            Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> updateAction)
            where TEntity : BaseEntity;

        /// <summary>
        /// Deletes entities matching a condition without loading them into memory (EF Core 7+).
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="predicate">The condition to match.</param>
        /// <returns>The number of affected rows.</returns>
        Task<int> DeleteFromQueryAsync<TEntity>(Expression<Func<TEntity, bool>> predicate)
            where TEntity : BaseEntity;

        #endregion Bulk Operations (EF Core 7+)

        #region Change Tracker

        /// <summary>
        /// Clears the change tracker. Use with caution: unsaved changes will be lost.
        /// </summary>
        void ClearChangeTracker();

        #endregion Change Tracker
    }
}