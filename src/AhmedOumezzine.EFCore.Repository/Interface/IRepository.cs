using AhmedOumezzine.EFCore.Repository.Entities;
using AhmedOumezzine.EFCore.Repository.Specification;
using Microsoft.EntityFrameworkCore.Query;
using System.Data.Common;
using System.Linq.Expressions;

namespace AhmedOumezzine.EFCore.Repository.Interface
{
    /// <summary>
    /// Unified interface for a generic repository supporting full CRUD, querying, bulk operations, and raw SQL.
    /// All methods automatically respect soft-delete (IsDeleted = false) unless explicitly stated.
    /// </summary>
    public interface IRepository
    {
         
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        #region ===== ADD / INSERT =====

        Task<object[]> InsertAsync<TEntity>(TEntity entity, CancellationToken ct = default)
            where TEntity : BaseEntity;

        Task InsertRangeAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken ct = default)
            where TEntity : BaseEntity;

        Task<TEntity> InsertAndReturnAsync<TEntity>(TEntity entity, CancellationToken ct = default)
            where TEntity : BaseEntity;

        Task InsertManyAsync<TEntity>(IEnumerable<TEntity> entities, int batchSize = 500, CancellationToken ct = default)
            where TEntity : BaseEntity;

        Task<object[]> InsertWithAuditAsync<TEntity>(TEntity entity, string userName, CancellationToken ct = default)
            where TEntity : BaseEntity;

        Task<bool> InsertIfNotExistsAsync<TEntity>(Expression<Func<TEntity, bool>> predicate, TEntity entity, CancellationToken ct = default)
            where TEntity : BaseEntity;

        Task<bool> TryInsertAsync<TEntity>(TEntity entity, CancellationToken ct = default)
            where TEntity : BaseEntity;

        Task UpsertAsync<TEntity>(Expression<Func<TEntity, bool>> predicate, TEntity entity, CancellationToken ct = default)
            where TEntity : BaseEntity;

        Task InsertWithTransactionAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken ct = default)
            where TEntity : BaseEntity;

        #endregion

        #region ===== DELETE / SOFT-DELETE =====

        void Remove<TEntity>(TEntity entity) where TEntity : BaseEntity;
        void Remove<TEntity>(IEnumerable<TEntity> entities) where TEntity : BaseEntity;

        int Delete<TEntity>(TEntity entity) where TEntity : BaseEntity;
        int Delete<TEntity>(IEnumerable<TEntity> entities) where TEntity : BaseEntity;

        Task<int> DeleteAsync<TEntity>(TEntity entity, CancellationToken ct = default) where TEntity : BaseEntity;
        Task<int> DeleteAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken ct = default) where TEntity : BaseEntity;

        Task<int> HardDeleteAsync<TEntity>(TEntity entity, CancellationToken ct = default) where TEntity : BaseEntity;
        Task<int> HardDeleteAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken ct = default) where TEntity : BaseEntity;

        Task<bool> DeleteIfExistsAsync<TEntity>(TEntity entity, CancellationToken ct = default) where TEntity : BaseEntity;
        Task<bool> DeleteByIdAsync<TEntity>(object id, CancellationToken ct = default) where TEntity : BaseEntity;

        Task<bool> TryDeleteAsync<TEntity>(TEntity entity, CancellationToken ct = default) where TEntity : BaseEntity;

        Task<int> RestoreAsync<TEntity>(TEntity entity, CancellationToken ct = default) where TEntity : BaseEntity;
        Task<int> RestoreRangeAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken ct = default) where TEntity : BaseEntity;
        Task<bool> RestoreByIdAsync<TEntity>(object id, CancellationToken ct = default) where TEntity : BaseEntity;
        Task<bool> TryRestoreAsync<TEntity>(TEntity entity, CancellationToken ct = default) where TEntity : BaseEntity;

        Task<int> DeleteRangeByConditionAsync<TEntity>(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default)
            where TEntity : BaseEntity;

        Task<int> PurgeSoftDeletedAsync<TEntity>(DateTime threshold, CancellationToken ct = default) where TEntity : BaseEntity;

        Task<TEntity?> DeleteAndReturnAsync<TEntity>(object id, CancellationToken ct = default) where TEntity : BaseEntity;

        #endregion

        #region ===== UPDATE =====

        void Update<TEntity>(TEntity entity) where TEntity : BaseEntity;
        void Update<TEntity>(IEnumerable<TEntity> entities) where TEntity : BaseEntity;

        Task<int> UpdateAsync<TEntity>(TEntity entity, CancellationToken ct = default) where TEntity : BaseEntity;
        Task<int> UpdateAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken ct = default) where TEntity : BaseEntity;

        Task<int> UpdateOnlyAsync<TEntity>(TEntity entity, string[] properties, CancellationToken ct = default)
            where TEntity : BaseEntity;

        Task<bool> UpdateIfExistsAsync<TEntity>(TEntity entity, CancellationToken ct = default) where TEntity : BaseEntity;

        Task<bool> TryUpdateAsync<TEntity>(TEntity entity, CancellationToken ct = default) where TEntity : BaseEntity;

        Task<int> UpdateFromQueryAsync<TEntity>(
            Expression<Func<TEntity, bool>> predicate,
            Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> updateAction)
            where TEntity : BaseEntity;

        #endregion

        #region ===== GET BY ID =====

        Task<TEntity?> GetByIdAsync<TEntity>(Guid id, CancellationToken ct = default) where TEntity : BaseEntity;
        Task<TEntity?> GetByIdAsync<TEntity>(Guid id, bool asNoTracking, CancellationToken ct = default) where TEntity : BaseEntity;
        Task<TEntity?> GetByIdAsync<TEntity>(
            Guid id,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> includes,
            CancellationToken ct = default) where TEntity : BaseEntity;
        Task<TEntity?> GetByIdAsync<TEntity>(
            Guid id,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> includes,
            bool asNoTracking,
            CancellationToken ct = default) where TEntity : BaseEntity;

        Task<List<TEntity>> GetByIdsAsync<TEntity>(IEnumerable<Guid> ids, bool asNoTracking = false, CancellationToken ct = default)
            where TEntity : BaseEntity;

        Task<TProjected?> GetProjectedByIdAsync<TEntity, TProjected>(
            Guid id,
            Expression<Func<TEntity, TProjected>> selector,
            CancellationToken ct = default)
            where TEntity : BaseEntity where TProjected : class;

        Task<TProperty> GetPropertyByIdAsync<TEntity, TProperty>(
            Guid id,
            Expression<Func<TEntity, TProperty>> propertySelector,
            CancellationToken ct = default)
            where TEntity : BaseEntity;

        Task<(bool Success, TEntity? Entity)> TryGetByIdAsync<TEntity>(Guid id, CancellationToken ct = default)
            where TEntity : BaseEntity;

        #endregion

        #region ===== GET SINGLE ENTITY =====

        Task<TEntity?> GetAsync<TEntity>(Expression<Func<TEntity, bool>> condition, CancellationToken ct = default)
            where TEntity : BaseEntity;
        Task<TEntity?> GetAsync<TEntity>(
            Expression<Func<TEntity, bool>> condition,
            bool asNoTracking,
            CancellationToken ct = default) where TEntity : BaseEntity;
        Task<TEntity?> GetAsync<TEntity>(
            Expression<Func<TEntity, bool>> condition,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> includes,
            CancellationToken ct = default) where TEntity : BaseEntity;
        Task<TEntity?> GetAsync<TEntity>(
            Expression<Func<TEntity, bool>> condition,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> includes,
            bool asNoTracking,
            CancellationToken ct = default) where TEntity : BaseEntity;

        Task<TEntity?> GetAsync<TEntity>(Specification<TEntity> specification, CancellationToken ct = default)
            where TEntity : BaseEntity;
        Task<TEntity?> GetAsync<TEntity>(Specification<TEntity> specification, bool asNoTracking, CancellationToken ct = default)
            where TEntity : BaseEntity;

        Task<TProjected?> GetAsync<TEntity, TProjected>(
            Expression<Func<TEntity, bool>> condition,
            Expression<Func<TEntity, TProjected>> selector,
            CancellationToken ct = default)
            where TEntity : BaseEntity where TProjected : class;

        Task<TProjected?> GetAsync<TEntity, TProjected>(
            Specification<TEntity> specification,
            Expression<Func<TEntity, TProjected>> selector,
            CancellationToken ct = default)
            where TEntity : BaseEntity where TProjected : class;

        Task<(bool Success, TEntity? Entity)> TryGetAsync<TEntity>(Expression<Func<TEntity, bool>> condition, CancellationToken ct = default)
            where TEntity : BaseEntity;

        Task<TEntity> GetFirstOrThrowAsync<TEntity>(
            Expression<Func<TEntity, bool>> condition,
            string? errorMessage = null,
            CancellationToken ct = default)
            where TEntity : BaseEntity;

        Task<(bool Exists, TEntity? Entity)> ExistsAndFetchAsync<TEntity>(
            Expression<Func<TEntity, bool>> condition,
            CancellationToken ct = default)
            where TEntity : BaseEntity;

        Task<TProperty> GetOnlyAsync<TEntity, TProperty>(
            Expression<Func<TEntity, bool>> condition,
            Expression<Func<TEntity, TProperty>> propertySelector,
            CancellationToken ct = default)
            where TEntity : BaseEntity;

        #endregion

        #region ===== GET LIST =====

        Task<List<TEntity>> GetListAsync<TEntity>(CancellationToken ct = default) where TEntity : BaseEntity;
        Task<List<TEntity>> GetListAsync<TEntity>(bool asNoTracking, CancellationToken ct = default) where TEntity : BaseEntity;
        Task<List<TEntity>> GetListAsync<TEntity>(
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> includes,
            CancellationToken ct = default) where TEntity : BaseEntity;
        Task<List<TEntity>> GetListAsync<TEntity>(
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> includes,
            bool asNoTracking,
            CancellationToken ct = default) where TEntity : BaseEntity;

        Task<List<TEntity>> GetListAsync<TEntity>(
            Expression<Func<TEntity, bool>> condition,
            CancellationToken ct = default) where TEntity : BaseEntity;
        Task<List<TEntity>> GetListAsync<TEntity>(
            Expression<Func<TEntity, bool>> condition,
            bool asNoTracking,
            CancellationToken ct = default) where TEntity : BaseEntity;
        Task<List<TEntity>> GetListAsync<TEntity>(
            Expression<Func<TEntity, bool>> condition,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> includes,
            bool asNoTracking,
            CancellationToken ct = default) where TEntity : BaseEntity;

        Task<List<TEntity>> GetListAsync<TEntity>(Specification<TEntity> specification, CancellationToken ct = default)
            where TEntity : BaseEntity;
        Task<List<TEntity>> GetListAsync<TEntity>(Specification<TEntity> specification, bool asNoTracking, CancellationToken ct = default)
            where TEntity : BaseEntity;

        Task<List<TProjected>> GetListAsync<TEntity, TProjected>(
            Expression<Func<TEntity, TProjected>> selector,
            CancellationToken ct = default)
            where TEntity : BaseEntity where TProjected : class;

        Task<List<TProjected>> GetListAsync<TEntity, TProjected>(
            Expression<Func<TEntity, bool>> condition,
            Expression<Func<TEntity, TProjected>> selector,
            CancellationToken ct = default)
            where TEntity : BaseEntity where TProjected : class;

        Task<List<TProjected>> GetListAsync<TEntity, TProjected>(
            Specification<TEntity> specification,
            Expression<Func<TEntity, TProjected>> selector,
            CancellationToken ct = default)
            where TEntity : BaseEntity where TProjected : class;

        Task<PaginatedList<TEntity>> GetListAsync<TEntity>(
            PaginationSpecification<TEntity> specification,
            CancellationToken ct = default)
            where TEntity : BaseEntity;

        Task<PaginatedList<TProjected>> GetListAsync<TEntity, TProjected>(
            PaginationSpecification<TEntity> specification,
            Expression<Func<TEntity, TProjected>> selector,
            CancellationToken ct = default)
            where TEntity : BaseEntity where TProjected : class;

        Task<List<TEntity>> GetActiveListAsync<TEntity>(CancellationToken ct = default) where TEntity : BaseEntity;
        Task<List<TEntity>> GetDeletedListAsync<TEntity>(
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> includes = null,
            bool asNoTracking = true,
            CancellationToken ct = default) where TEntity : BaseEntity;

        Task<(bool Success, List<TEntity> Items)> TryGetListAsync<TEntity>(
            Expression<Func<TEntity, bool>> condition = null,
            CancellationToken ct = default) where TEntity : BaseEntity;

        Task<List<TKey>> GetDistinctByAsync<TEntity, TKey>(
            Expression<Func<TEntity, TKey>> keySelector,
            CancellationToken ct = default) where TEntity : BaseEntity;

        Task<(bool HasAny, List<TEntity> Items)> ExistsAnyAndListAsync<TEntity>(
            Expression<Func<TEntity, bool>> condition = null,
            CancellationToken ct = default) where TEntity : BaseEntity;

        #endregion

        #region ===== EXISTS / COUNT =====

        Task<bool> ExistsAsync<TEntity>(CancellationToken ct = default) where TEntity : BaseEntity;
        Task<bool> ExistsAsync<TEntity>(Expression<Func<TEntity, bool>> condition, CancellationToken ct = default)
            where TEntity : BaseEntity;
        Task<bool> ExistsByIdAsync<TEntity>(object id, CancellationToken ct = default) where TEntity : BaseEntity;

        Task<int> CountAsync<TEntity>(CancellationToken ct = default) where TEntity : BaseEntity;
        Task<int> CountAsync<TEntity>(Expression<Func<TEntity, bool>> condition, CancellationToken ct = default)
            where TEntity : BaseEntity;

        Task<long> GetLongCountAsync<TEntity>(CancellationToken ct = default) where TEntity : BaseEntity;
        Task<long> GetLongCountAsync<TEntity>(Expression<Func<TEntity, bool>> condition, CancellationToken ct = default)
            where TEntity : BaseEntity;

        Task<int> CountSoftDeletedAsync<TEntity>(CancellationToken ct = default) where TEntity : BaseEntity;

        Task<Dictionary<bool, int>> CountByStatusAsync<TEntity>(
            Expression<Func<TEntity, bool>> statusSelector,
            CancellationToken ct = default) where TEntity : BaseEntity;

        Task<int> CountByDateRangeAsync<TEntity>(
            DateTime startDate,
            DateTime endDate,
            CancellationToken ct = default) where TEntity : BaseEntity;

        #endregion

        #region ===== RAW SQL =====

        Task<int> ExecuteSqlCommandAsync(string sql, CancellationToken ct = default);
        Task<int> ExecuteSqlCommandAsync(string sql, params object[] parameters);
        Task<int> ExecuteSqlCommandAsync(string sql, IEnumerable<object> parameters, CancellationToken ct = default);

        Task<List<T>> GetFromRawSqlAsync<T>(string sql, CancellationToken ct = default) where T : class;
        Task<List<T>> GetFromRawSqlAsync<T>(string sql, params object[] parameters) where T : class;
        Task<List<T>> GetFromRawSqlAsync<T>(string sql, IEnumerable<object> parameters, CancellationToken ct = default) where T : class;
        Task<List<T>> GetFromRawSqlAsync<T>(string sql, IEnumerable<DbParameter> parameters, CancellationToken ct = default) where T : class;

        Task<T> ExecuteScalarAsync<T>(string sql, IEnumerable<object> parameters = null, CancellationToken ct = default);

        Task<T?> GetSingleFromSqlAsync<T>(string sql, IEnumerable<object> parameters = null, CancellationToken ct = default) where T : class;

        Task<bool> ExistsBySqlAsync(string sql, IEnumerable<object> parameters = null, CancellationToken ct = default);

        Task<int> ExecuteInTransactionAsync(string sql, IEnumerable<object> parameters = null, CancellationToken ct = default);

        #endregion
    }
}