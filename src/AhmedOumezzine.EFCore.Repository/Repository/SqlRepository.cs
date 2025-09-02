using AhmedOumezzine.EFCore.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;

namespace AhmedOumezzine.EFCore.Repository.Repository
{
    /// <summary>
    /// Partial implementation of the generic repository for executing raw SQL commands and queries.
    /// Supports scalar values, stored procedures, transactions, and result mapping.
    /// </summary>
    /// <typeparam name="TDbContext">The type of the database context.</typeparam>
    public sealed partial class Repository<TDbContext> : IRepository
        where TDbContext : DbContext
    {
        #region ExecuteSqlCommandAsync - Raw SQL Commands

        /// <summary>
        /// Executes a raw SQL command (INSERT, UPDATE, DELETE, etc.) and returns the number of affected rows.
        /// </summary>
        /// <param name="sql">The SQL command to execute. Cannot be null or empty.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The number of affected rows.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="sql"/> is null or whitespace.</exception>
        public Task<int> ExecuteSqlCommandAsync(
            string sql,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentNullException(nameof(sql));

            return _dbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
        }

        /// <summary>
        /// Executes a parameterized raw SQL command.
        /// </summary>
        /// <param name="sql">The SQL command with parameters (e.g., 'UPDATE Users SET Name = {0} WHERE Id = {1}').</param>
        /// <param name="parameters">The parameters to pass into the SQL command.</param>
        /// <returns>The number of affected rows.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="sql"/> is null or whitespace.</exception>
        public Task<int> ExecuteSqlCommandAsync(
            string sql,
            params object[] parameters)
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentNullException(nameof(sql));

            return _dbContext.Database.ExecuteSqlRawAsync(sql, parameters);
        }

        /// <summary>
        /// Executes a parameterized raw SQL command with cancellation support.
        /// </summary>
        /// <param name="sql">The SQL command with parameters.</param>
        /// <param name="parameters">The parameters to pass into the SQL command.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The number of affected rows.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="sql"/> is null or whitespace.</exception>
        public Task<int> ExecuteSqlCommandAsync(
            string sql,
            IEnumerable<object> parameters,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentNullException(nameof(sql));

            return _dbContext.Database.ExecuteSqlRawAsync(sql, parameters, cancellationToken);
        }

        #endregion ExecuteSqlCommandAsync - Raw SQL Commands

        #region GetFromRawSqlAsync - Raw SQL Queries (SELECT)

        /// <summary>
        /// Executes a raw SQL SELECT query and maps the results to a list of entities.
        /// </summary>
        /// <typeparam name="T">The entity type. Must be part of the DbContext model.</typeparam>
        /// <param name="sql">The SQL query to execute (e.g., 'SELECT * FROM Users WHERE Status = {0}').</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>A list of entities mapped from the query results.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="sql"/> is null or whitespace.</exception>
        public async Task<List<T>> GetFromRawSqlAsync<T>(
            string sql,
            CancellationToken cancellationToken = default)
            where T : class
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentNullException(nameof(sql));

            var query = _dbContext.Set<T>().FromSqlRaw(sql);
            return await query.ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Executes a parameterized raw SQL SELECT query.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="sql">The SQL query with parameters.</param>
        /// <param name="parameters">The parameters to pass into the query.</param>
        /// <returns>A list of entities.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="sql"/> is null or whitespace.</exception>
        public async Task<List<T>> GetFromRawSqlAsync<T>(
            string sql,
            params object[] parameters)
            where T : class
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentNullException(nameof(sql));

            var query = _dbContext.Set<T>().FromSqlRaw(sql, parameters);
            return await query.ToListAsync();
        }

        /// <summary>
        /// Executes a parameterized raw SQL SELECT query with cancellation support.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="sql">The SQL query with parameters.</param>
        /// <param name="parameters">The parameters to pass into the query.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>A list of entities.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="sql"/> is null or whitespace.</exception>
        public async Task<List<T>> GetFromRawSqlAsync<T>(
            string sql,
            IEnumerable<object> parameters,
            CancellationToken cancellationToken = default)
            where T : class
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentNullException(nameof(sql));

            var query = _dbContext.Set<T>().FromSqlRaw(sql, parameters);
            return await query.ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Executes a raw SQL query with DbParameter objects (for named parameters).
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="sql">The SQL query with named parameters (e.g., 'SELECT * FROM Users WHERE Status = @status').</param>
        /// <param name="parameters">The DbParameter objects (e.g., SqlParameter).</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>A list of entities.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="sql"/> is null or whitespace.</exception>
        public async Task<List<T>> GetFromRawSqlAsync<T>(
            string sql,
            IEnumerable<DbParameter> parameters,
            CancellationToken cancellationToken = default)
            where T : class
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentNullException(nameof(sql));

            var query = _dbContext.Set<T>().FromSqlRaw(sql, parameters);
            return await query.ToListAsync(cancellationToken);
        }

        #endregion GetFromRawSqlAsync - Raw SQL Queries (SELECT)

        #region QueryFromSqlAsync (Semantic Alias)

        /// <summary>
        /// Executes a raw SQL query and maps results to a list of entities.
        /// Alias for GetFromRawSqlAsync with clearer naming.
        /// </summary>
        public Task<List<T>> QueryFromSqlAsync<T>(
            string sql,
            params object[] parameters)
            where T : class
        {
            return GetFromRawSqlAsync<T>(sql, parameters);
        }

        #endregion QueryFromSqlAsync (Semantic Alias)

        #region ExecuteScalarAsync (Single Value)

        /// <summary>
        /// Executes a SQL query and returns a single scalar value (e.g., COUNT, MAX, custom).
        /// </summary>
        /// <typeparam name="T">The type of the scalar result (e.g., int, string, DateTime).</typeparam>
        /// <param name="sql">The SQL query (e.g., "SELECT COUNT(*) FROM Users").</param>
        /// <param name="parameters">Optional parameters.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The scalar value, or default(T) if no result.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="sql"/> is null or whitespace.</exception>
        public async Task<T> ExecuteScalarAsync<T>(
            string sql,
            IEnumerable<object> parameters = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentNullException(nameof(sql));

            var command = _dbContext.Database.GetDbConnection().CreateCommand();
            command.CommandText = sql;
            command.CommandType = CommandType.Text;

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    var dbParam = command.CreateParameter();
                    dbParam.Value = param ?? DBNull.Value;
                    command.Parameters.Add(dbParam);
                }
            }

            if (command.Connection.State != ConnectionState.Open)
                await command.Connection.OpenAsync(cancellationToken);

            var result = await command.ExecuteScalarAsync(cancellationToken);
            return result switch
            {
                null or DBNull => default(T),
                T value => value,
                _ => (T)Convert.ChangeType(result, typeof(T))
            };
        }

        #endregion ExecuteScalarAsync (Single Value)

        #region GetSingleFromSqlAsync (First or Default)

        /// <summary>
        /// Executes a raw SQL query and returns the first entity or null.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="sql">The SQL query.</param>
        /// <param name="parameters">Optional parameters.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The first entity or null.</returns>
        public async Task<T> GetSingleFromSqlAsync<T>(
            string sql,
            IEnumerable<object> parameters = null,
            CancellationToken cancellationToken = default)
            where T : class
        {
            var list = await GetFromRawSqlAsync<T>(sql, parameters, cancellationToken);
            return list.FirstOrDefault();
        }

        #endregion GetSingleFromSqlAsync (First or Default)

        #region ExistsBySqlAsync (Existence Check)

        /// <summary>
        /// Checks if any row matches the given SQL query.
        /// </summary>
        /// <param name="sql">The SQL query that returns a count or boolean.</param>
        /// <param name="parameters">Optional parameters.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>True if at least one row matches; otherwise, false.</returns>
        public async Task<bool> ExistsBySqlAsync(
            string sql,
            IEnumerable<object> parameters = null,
            CancellationToken cancellationToken = default)
        {
            var count = await ExecuteScalarAsync<int>(sql, parameters, cancellationToken);
            return count > 0;
        }

        #endregion ExistsBySqlAsync (Existence Check)

        #region ExecuteStoredProcedureAsync

        /// <summary>
        /// Executes a stored procedure and returns the number of affected rows.
        /// </summary>
        /// <param name="procedureName">The name of the stored procedure.</param>
        /// <param name="parameters">Optional parameters.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The number of affected rows.</returns>
        public Task<int> ExecuteStoredProcedureAsync(
            string procedureName,
            IEnumerable<object> parameters = null,
            CancellationToken cancellationToken = default)
        {
            var sql = $"EXEC {procedureName}";
            if (parameters?.Any() == true)
            {
                var paramPlaceholders = string.Join(", ", parameters.Select((_, i) => $"{{{i}}}"));
                sql += $" {paramPlaceholders}";
            }

            return parameters != null
                ? _dbContext.Database.ExecuteSqlRawAsync(sql, parameters, cancellationToken)
                : _dbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
        }

        #endregion ExecuteStoredProcedureAsync

        #region ExecuteInTransactionAsync

        /// <summary>
        /// Executes a SQL command within a transaction.
        /// </summary>
        /// <param name="sql">The SQL command to execute.</param>
        /// <param name="parameters">Optional parameters.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The number of affected rows.</returns>
        public async Task<int> ExecuteInTransactionAsync(
            string sql,
            IEnumerable<object> parameters = null,
            CancellationToken cancellationToken = default)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var result = await ExecuteSqlCommandAsync(sql, parameters, cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return result;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        #endregion ExecuteInTransactionAsync
    }
}