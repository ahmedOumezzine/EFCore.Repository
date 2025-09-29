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
    public sealed partial class Repository<TDbContext> : IRepository
        where TDbContext : DbContext
    {
        #region ExecuteSqlCommandAsync - Raw SQL Commands

        /// <summary>
        /// Executes a raw SQL command (INSERT, UPDATE, DELETE) and returns the number of affected rows.
        /// </summary>
        public Task<int> ExecuteSqlCommandAsync(
            string sql,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentNullException(nameof(sql));

            return _dbContext.Database.ExecuteSqlRawAsync(sql, ct);
        }

        /// <summary>
        /// Executes a parameterized raw SQL command.
        /// </summary>
        public Task<int> ExecuteSqlCommandAsync(
            string sql,
            params object[] parameters)
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentNullException(nameof(sql));

            return _dbContext.Database.ExecuteSqlRawAsync(sql, parameters);
        }

        /// <summary>
        /// Executes a parameterized raw SQL command with cancellation.
        /// </summary>
        public Task<int> ExecuteSqlCommandAsync(
            string sql,
            IEnumerable<object> parameters,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentNullException(nameof(sql));

            return _dbContext.Database.ExecuteSqlRawAsync(sql, parameters, ct);
        }

        #endregion ExecuteSqlCommandAsync - Raw SQL Commands

        #region GetFromRawSqlAsync - Raw SQL Queries (SELECT)

        /// <summary>
        /// Executes a raw SQL SELECT query and maps results to entities.
        /// </summary>
        public async Task<List<T>> GetFromRawSqlAsync<T>(
            string sql,
            CancellationToken ct = default)
            where T : class
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentNullException(nameof(sql));

            return await _dbContext.Set<T>()
                .FromSqlRaw(sql)
                .ToListAsync(ct);
        }

        public async Task<List<T>> GetFromRawSqlAsync<T>(
            string sql,
            params object[] parameters)
            where T : class
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentNullException(nameof(sql));

            return await _dbContext.Set<T>()
                .FromSqlRaw(sql, parameters)
                .ToListAsync();
        }

        public async Task<List<T>> GetFromRawSqlAsync<T>(
            string sql,
            IEnumerable<object> parameters,
            CancellationToken ct = default)
            where T : class
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentNullException(nameof(sql));

            return await _dbContext.Set<T>()
                .FromSqlRaw(sql, parameters)
                .ToListAsync(ct);
        }

        public async Task<List<T>> GetFromRawSqlAsync<T>(
            string sql,
            IEnumerable<DbParameter> parameters,
            CancellationToken ct = default)
            where T : class
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentNullException(nameof(sql));

            return await _dbContext.Set<T>()
                .FromSqlRaw(sql, parameters)
                .ToListAsync(ct);
        }

        #endregion GetFromRawSqlAsync - Raw SQL Queries (SELECT)

        #region ExecuteScalarAsync - Single Value

        /// <summary>
        /// Executes a SQL query and returns a single scalar value.
        /// </summary>
        public async Task<T> ExecuteScalarAsync<T>(
            string sql,
            IEnumerable<object> parameters = null,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentNullException(nameof(sql));

            var connection = _dbContext.Database.GetDbConnection();
            var wasClosed = connection.State == ConnectionState.Closed;

            if (wasClosed)
                await connection.OpenAsync(ct);

            try
            {
                using var command = connection.CreateCommand();
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

                var result = await command.ExecuteScalarAsync(ct);
                return result switch
                {
                    null or DBNull => default(T),
                    T value => value,
                    _ => (T)Convert.ChangeType(result, typeof(T))
                };
            }
            finally
            {
                if (wasClosed)
                    await connection.CloseAsync();
            }
        }

        #endregion ExecuteScalarAsync - Single Value

        #region GetSingleFromSqlAsync - First Entity Only

        /// <summary>
        /// Executes a raw SQL query and returns the first entity or null.
        /// </summary>
        public async Task<T?> GetSingleFromSqlAsync<T>(
            string sql,
            IEnumerable<object> parameters = null,
            CancellationToken ct = default)
            where T : class
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentNullException(nameof(sql));

            var query = _dbContext.Set<T>().FromSqlRaw(sql, parameters ?? Array.Empty<object>());
            var list = await query.Take(1).ToListAsync(ct);
            return list.FirstOrDefault();
        }

        #endregion GetSingleFromSqlAsync - First Entity Only

        #region ExistsBySqlAsync - Existence Check

        /// <summary>
        /// Checks if any row matches the SQL query.
        /// </summary>
        public async Task<bool> ExistsBySqlAsync(
            string sql,
            IEnumerable<object> parameters = null,
            CancellationToken ct = default)
        {
            var count = await ExecuteScalarAsync<int>(sql, parameters, ct);
            return count > 0;
        }

        #endregion ExistsBySqlAsync - Existence Check

        #region ExecuteInTransactionAsync

        /// <summary>
        /// Executes a SQL command within a transaction.
        /// </summary>
        public async Task<int> ExecuteInTransactionAsync(
            string sql,
            IEnumerable<object> parameters = null,
            CancellationToken ct = default)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);
            try
            {
                var result = await ExecuteSqlCommandAsync(sql, parameters, ct);
                await transaction.CommitAsync(ct);
                return result;
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }

        #endregion ExecuteInTransactionAsync

        #region (Removed) ExecuteStoredProcedureAsync

        // ⚠️ Removed due to SQL injection risk.
        // If needed, validate procedureName against a whitelist.

        #endregion (Removed) ExecuteStoredProcedureAsync
    }
}