using AhmedOumezzine.EFCore.Repository.Entities;
using AhmedOumezzine.EFCore.Repository.Extensions;
using AhmedOumezzine.EFCore.Repository.Specification;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace AhmedOumezzine.EFCore.Repository.Interface
{
    /// <summary>
    /// Defines a contract for querying entities from the database.
    /// Supports filtering, pagination, projection, specifications, and raw SQL.
    /// All generic type parameters are properly constrained to ensure consistency with implementation.
    /// </summary>
    public interface IQueryRepository
    {
        #region Queryable

        /// <summary>
        /// Gets a queryable set of entities.
        /// </summary>
        IQueryable<TEntity> GetQueryable<TEntity>() where TEntity : BaseEntity;

        #endregion

        #region List - GetAll & Filtered

        /// <summary>
        /// Retrieves all non-deleted entities of the specified type.
        /// </summary>
        Task<List<TEntity>> GetListAsync<TEntity>(CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        /// <summary>
        /// Retrieves all non-deleted entities with optional tracking control.
        /// </summary>
        Task<List<TEntity>> GetListAsync<TEntity>(bool asNoTracking, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        /// <summary>
        /// Retrieves all non-deleted entities with optional includes.
        /// </summary>
        Task<List<TEntity>> GetListAsync<TEntity>(
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> includes,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        /// <summary>
        /// Retrieves all non-deleted entities with includes and optional tracking.
        /// </summary>
        Task<List<TEntity>> GetListAsync<TEntity>(
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> includes,
            bool asNoTracking,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        /// <summary>
        /// Retrieves entities matching the condition.
        /// </summary>
        Task<List<TEntity>> GetListAsync<TEntity>(
            Expression<Func<TEntity, bool>> condition,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        /// <summary>
        /// Retrieves entities matching the condition with optional tracking.
        /// </summary>
        Task<List<TEntity>> GetListAsync<TEntity>(
            Expression<Func<TEntity, bool>> condition,
            bool asNoTracking,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        /// <summary>
        /// Retrieves entities matching the condition with includes and optional tracking.
        /// </summary>
        Task<List<TEntity>> GetListAsync<TEntity>(
            Expression<Func<TEntity, bool>> condition,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> includes,
            bool asNoTracking,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        #endregion

        #region List - With Specification

        /// <summary>
        /// Retrieves entities matching the specification.
        /// </summary>
        Task<List<TEntity>> GetListAsync<TEntity>(
            Specification<TEntity> specification,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        /// <summary>
        /// Retrieves entities matching the specification with optional tracking.
        /// </summary>
        Task<List<TEntity>> GetListAsync<TEntity>(
            Specification<TEntity> specification,
            bool asNoTracking,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        #endregion

        #region List - Projection

        /// <summary>
        /// Retrieves a list of projected entities (e.g., DTOs).
        /// </summary>
        Task<List<TProjectedType>> GetListAsync<TEntity, TProjectedType>(
            Expression<Func<TEntity, TProjectedType>> selectExpression,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
            where TProjectedType : class;

        /// <summary>
        /// Retrieves projected entities matching a condition.
        /// </summary>
        Task<List<TProjectedType>> GetListAsync<TEntity, TProjectedType>(
            Expression<Func<TEntity, bool>> condition,
            Expression<Func<TEntity, TProjectedType>> selectExpression,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
            where TProjectedType : class;

        /// <summary>
        /// Retrieves projected entities matching a specification.
        /// </summary>
        Task<List<TProjectedType>> GetListAsync<TEntity, TProjectedType>(
            Specification<TEntity> specification,
            Expression<Func<TEntity, TProjectedType>> selectExpression,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
            where TProjectedType : class;

        #endregion

        #region List - Pagination

        /// <summary>
        /// Retrieves a paginated list of entities.
        /// </summary>
        Task<PaginatedList<TEntity>> GetListAsync<TEntity>(
            PaginationSpecification<TEntity> specification,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        /// <summary>
        /// Retrieves a paginated list of projected entities.
        /// </summary>
        Task<PaginatedList<TProjectedType>> GetListAsync<TEntity, TProjectedType>(
            PaginationSpecification<TEntity> specification,
            Expression<Func<TEntity, TProjectedType>> selectExpression,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
            where TProjectedType : class;

        #endregion

        #region GetById

        /// <summary>
        /// Retrieves an entity by its primary key.
        /// </summary>
        Task<TEntity> GetByIdAsync<TEntity>(Guid? id, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        /// <summary>
        /// Retrieves an entity by its primary key with optional tracking.
        /// </summary>
        Task<TEntity> GetByIdAsync<TEntity>(Guid? id, bool asNoTracking, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        /// <summary>
        /// Retrieves an entity by its primary key with includes.
        /// </summary>
        Task<TEntity> GetByIdAsync<TEntity>(
            Guid? id,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> includes,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        /// <summary>
        /// Retrieves an entity by its primary key with includes and optional tracking.
        /// </summary>
        Task<TEntity> GetByIdAsync<TEntity>(
            Guid? id,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> includes,
            bool asNoTracking,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        /// <summary>
        /// Retrieves a projected entity by its primary key.
        /// </summary>
        Task<TProjectedType> GetByIdAsync<TEntity, TProjectedType>(
            Guid? id,
            Expression<Func<TEntity, TProjectedType>> selectExpression,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
            where TProjectedType : class;

        #endregion

        #region Get (First or Default)

        /// <summary>
        /// Retrieves the first entity matching the condition.
        /// </summary>
        Task<TEntity> GetAsync<TEntity>(
            Expression<Func<TEntity, bool>> condition,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        /// <summary>
        /// Retrieves the first entity matching the condition with optional tracking.
        /// </summary>
        Task<TEntity> GetAsync<TEntity>(
            Expression<Func<TEntity, bool>> condition,
            bool asNoTracking,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        /// <summary>
        /// Retrieves the first entity matching the condition with includes.
        /// </summary>
        Task<TEntity> GetAsync<TEntity>(
            Expression<Func<TEntity, bool>> condition,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> includes,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        /// <summary>
        /// Retrieves the first entity matching the condition with includes and optional tracking.
        /// </summary>
        Task<TEntity> GetAsync<TEntity>(
            Expression<Func<TEntity, bool>> condition,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> includes,
            bool asNoTracking,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        /// <summary>
        /// Retrieves the first entity matching the specification.
        /// </summary>
        Task<TEntity> GetAsync<TEntity>(
            Specification<TEntity> specification,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        /// <summary>
        /// Retrieves the first entity matching the specification with optional tracking.
        /// </summary>
        Task<TEntity> GetAsync<TEntity>(
            Specification<TEntity> specification,
            bool asNoTracking,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        /// <summary>
        /// Retrieves a projected entity based on a condition.
        /// </summary>
        Task<TProjectedType> GetAsync<TEntity, TProjectedType>(
            Expression<Func<TEntity, bool>> condition,
            Expression<Func<TEntity, TProjectedType>> selectExpression,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
            where TProjectedType : class;

        /// <summary>
        /// Retrieves a projected entity based on a specification.
        /// </summary>
        Task<TProjectedType> GetAsync<TEntity, TProjectedType>(
            Specification<TEntity> specification,
            Expression<Func<TEntity, TProjectedType>> selectExpression,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
            where TProjectedType : class;

        #endregion

        #region Exists

        /// <summary>
        /// Checks if any entity of the specified type exists.
        /// </summary>
        Task<bool> ExistsAsync<TEntity>(CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        /// <summary>
        /// Checks if any entity matching the condition exists.
        /// </summary>
        Task<bool> ExistsAsync<TEntity>(
            Expression<Func<TEntity, bool>> condition,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        /// <summary>
        /// Checks if an entity with the given ID exists.
        /// </summary>
        Task<bool> ExistsByIdAsync<TEntity>(Guid? id, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        #endregion

        #region Count

        /// <summary>
        /// Gets the count of entities of the specified type.
        /// </summary>
        Task<int> GetCountAsync<TEntity>(CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        /// <summary>
        /// Gets the count of entities matching the condition.
        /// </summary>
        Task<int> GetCountAsync<TEntity>(
            Expression<Func<TEntity, bool>> condition,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        /// <summary>
        /// Gets the count of entities matching all conditions.
        /// </summary>
        Task<int> GetCountAsync<TEntity>(
            IEnumerable<Expression<Func<TEntity, bool>>> conditions,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        /// <summary>
        /// Gets the long count of entities.
        /// </summary>
        Task<long> GetLongCountAsync<TEntity>(CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        /// <summary>
        /// Gets the long count of entities matching the condition.
        /// </summary>
        Task<long> GetLongCountAsync<TEntity>(
            Expression<Func<TEntity, bool>> condition,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        /// <summary>
        /// Gets the long count of entities matching all conditions.
        /// </summary>
        Task<long> GetLongCountAsync<TEntity>(
            IEnumerable<Expression<Func<TEntity, bool>>> conditions,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        #endregion

        #region Raw SQL Queries

        /// <summary>
        /// Executes a raw SQL query and returns a list of entities.
        /// </summary>
        Task<List<T>> GetFromRawSqlAsync<T>(
            string sql,
            CancellationToken cancellationToken = default)
            where T : class;

        /// <summary>
        /// Executes a parameterized raw SQL query.
        /// </summary>
        Task<List<T>> GetFromRawSqlAsync<T>(
            string sql,
            params object[] parameters)
            where T : class;

        /// <summary>
        /// Executes a parameterized raw SQL query with cancellation.
        /// </summary>
        Task<List<T>> GetFromRawSqlAsync<T>(
            string sql,
            IEnumerable<object> parameters,
            CancellationToken cancellationToken = default)
            where T : class;

        /// <summary>
        /// Executes a raw SQL query with DbParameter objects (named parameters).
        /// </summary>
        Task<List<T>> GetFromRawSqlAsync<T>(
            string sql,
            IEnumerable<DbParameter> parameters,
            CancellationToken cancellationToken = default)
            where T : class;

        #endregion
    }
}