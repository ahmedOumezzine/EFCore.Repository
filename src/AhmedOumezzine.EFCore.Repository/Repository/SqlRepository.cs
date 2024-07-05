using AhmedOumezzine.EFCore.Repository.Extensions;
using AhmedOumezzine.EFCore.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace AhmedOumezzine.EFCore.Repository.Repository
{
    public sealed partial class Repository<TDbContext> : IRepository
        where TDbContext : DbContext
    {
        /// <summary>
        /// Executes the specified SQL query asynchronously against the database.
        /// </summary>
        /// <param name="sql">The SQL query to execute.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result represents the number of rows affected.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the SQL query is null or empty.</exception>
        public Task<int> ExecuteSqlCommandAsync(string sql,
                                                CancellationToken cancellationToken = default)
        {
            return _dbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
        }

        /// <summary>
        /// Executes the specified SQL query asynchronously against the database with parameters.
        /// </summary>
        /// <param name="sql">The SQL query to execute.</param>
        /// <param name="parameters">The parameters to pass into the SQL query.</param>
        /// <returns>A task that represents the asynchronous operation. The task result represents the number of rows affected.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the SQL query is null or empty.</exception>
        public Task<int> ExecuteSqlCommandAsync(string sql,
                                                params object[] parameters)
        {
            return _dbContext.Database.ExecuteSqlRawAsync(sql, parameters);
        }

        /// <summary>
        /// Executes the specified SQL query asynchronously against the database with parameters and a cancellation token.
        /// </summary>
        /// <param name="sql">The SQL query to execute.</param>
        /// <param name="parameters">The parameters to pass into the SQL query.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result represents the number of rows affected.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the SQL query is null or empty.</exception>
        public Task<int> ExecuteSqlCommandAsync(string sql,
                                                IEnumerable<object> parameters,
                                                CancellationToken cancellationToken = default)
        {
            return _dbContext.Database.ExecuteSqlRawAsync(sql, parameters, cancellationToken);
        }

        /// <summary>
        /// Executes a raw SQL query asynchronously and retrieves a list of entities of type T from the database.
        /// </summary>
        /// <typeparam name="T">The type of entities to retrieve.</typeparam>
        /// <param name="sql">The raw SQL query to execute.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of entities retrieved from the database.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the SQL query is null or empty.</exception>
        public async Task<List<T>> GetFromRawSqlAsync<T>(string sql,
                                                         CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(sql))
            {
                throw new ArgumentNullException(nameof(sql));
            }

            IEnumerable<object> parameters = new List<object>();

            List<T> items = await _dbContext.GetFromQueryAsync<T>(sql, parameters, cancellationToken);
            return items;
        }

        /// <summary>
        /// Executes a raw SQL query asynchronously and retrieves a list of entities of type T from the database.
        /// </summary>
        /// <typeparam name="T">The type of entities to retrieve.</typeparam>
        /// <param name="sql">The raw SQL query to execute.</param>
        /// <param name="parameters">The parameters to pass into the SQL query.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of entities retrieved from the database.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the SQL query is null or empty.</exception>
        public async Task<List<T>> GetFromRawSqlAsync<T>(string sql,
                                                         object parameter,
                                                         CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(sql))
            {
                throw new ArgumentNullException(nameof(sql));
            }

            List<object> parameters = new List<object>() { parameter };
            List<T> items = await _dbContext.GetFromQueryAsync<T>(sql, parameters, cancellationToken);
            return items;
        }

        /// <summary>
        /// Executes a raw SQL query asynchronously and retrieves a list of entities of type T from the database.
        /// </summary>
        /// <typeparam name="T">The type of entities to retrieve.</typeparam>
        /// <param name="sql">The raw SQL query to execute.</param>
        /// <param name="parameters">The parameters to pass into the SQL query.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of entities retrieved from the database.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the SQL query is null or empty.</exception>
        public async Task<List<T>> GetFromRawSqlAsync<T>(string sql,
                                                         IEnumerable<DbParameter> parameters,
                                                         CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(sql))
            {
                throw new ArgumentNullException(nameof(sql));
            }

            List<T> items = await _dbContext.GetFromQueryAsync<T>(sql, parameters, cancellationToken);
            return items;
        }

        /// <summary>
        /// Executes a raw SQL query asynchronously and retrieves a list of entities of type T from the database.
        /// </summary>
        /// <typeparam name="T">The type of entities to retrieve.</typeparam>
        /// <param name="sql">The raw SQL query to execute.</param>
        /// <param name="parameters">The parameters to pass into the SQL query.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of entities retrieved from the database.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the SQL query is null or empty.</exception>
        public async Task<List<T>> GetFromRawSqlAsync<T>(string sql,
                                                         IEnumerable<object> parameters,
                                                         CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(sql))
            {
                throw new ArgumentNullException(nameof(sql));
            }

            List<T> items = await _dbContext.GetFromQueryAsync<T>(sql, parameters, cancellationToken);
            return items;
        }
    }
}