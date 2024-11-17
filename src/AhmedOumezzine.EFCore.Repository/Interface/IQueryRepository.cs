using AhmedOumezzine.EFCore.Repository.Entities;
using AhmedOumezzine.EFCore.Repository.Extensions;
using AhmedOumezzine.EFCore.Repository.Specification;
using Microsoft.EntityFrameworkCore.Query;
using System.Data.Common;
using System.Linq.Expressions;

namespace AhmedOumezzine.EFCore.Repository.Interface
{
    public interface IQueryRepository
    {
        IQueryable<TEntity> GetQueryable<TEntity>()
            where TEntity : BaseEntity;

        Task<List<TEntity>> GetListAsync<TEntity>(CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        Task<List<TEntity>> GetListAsync<TEntity>(bool asNoTracking, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        Task<List<TEntity>> GetListAsync<TEntity>(
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> includes,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        Task<List<TEntity>> GetListAsync<TEntity>(
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> includes,
            bool asNoTracking,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        Task<List<TEntity>> GetListAsync<TEntity>(Expression<Func<TEntity, bool>> condition, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        Task<List<TEntity>> GetListAsync<TEntity>(
            Expression<Func<TEntity, bool>> condition,
            bool asNoTracking,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        Task<List<TEntity>> GetListAsync<TEntity>(
            Expression<Func<TEntity, bool>> condition,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> includes,
            bool asNoTracking = false,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        Task<List<TEntity>> GetListAsync<TEntity>(Specification<TEntity> specification, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        Task<List<TEntity>> GetListAsync<TEntity>(
            Specification<TEntity> specification,
            bool asNoTracking,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        Task<List<TProjectedType>> GetListAsync<TEntity, TProjectedType>(
            Expression<Func<TEntity, TProjectedType>> selectExpression,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        Task<List<TProjectedType>> GetListAsync<TEntity, TProjectedType>(
          Expression<Func<TEntity, bool>> condition,
          Expression<Func<TEntity, TProjectedType>> selectExpression,
          CancellationToken cancellationToken = default)
          where TEntity : BaseEntity;

        Task<List<TProjectedType>> GetListAsync<TEntity, TProjectedType>(
         Specification<TEntity> specification,
         Expression<Func<TEntity, TProjectedType>> selectExpression,
         CancellationToken cancellationToken = default)
         where TEntity : BaseEntity;

        Task<PaginatedList<TEntity>> GetListAsync<TEntity>(
         PaginationSpecification<TEntity> specification,
         CancellationToken cancellationToken = default)
         where TEntity : BaseEntity;

        Task<PaginatedList<TProjectedType>> GetListAsync<TEntity, TProjectedType>(
         PaginationSpecification<TEntity> specification,
         Expression<Func<TEntity, TProjectedType>> selectExpression,
         CancellationToken cancellationToken = default)
         where TEntity : BaseEntity
         where TProjectedType : class;

        Task<TEntity> GetByIdAsync<TEntity>(Guid? id, CancellationToken cancellationToken = default)
         where TEntity : BaseEntity;

        Task<TEntity> GetByIdAsync<TEntity>(Guid? id, bool asNoTracking, CancellationToken cancellationToken = default)
           where TEntity : BaseEntity;

        Task<TEntity> GetByIdAsync<TEntity>(
          Guid? id,
          Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> includes,
          CancellationToken cancellationToken = default)
          where TEntity : BaseEntity;

        Task<TEntity> GetByIdAsync<TEntity>(
            Guid? id,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> includes,
            bool asNoTracking,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        Task<TProjectedType> GetByIdAsync<TEntity, TProjectedType>(
           Guid? id,
           Expression<Func<TEntity, TProjectedType>> selectExpression,
           CancellationToken cancellationToken = default)
           where TEntity : BaseEntity;

        Task<TEntity> GetAsync<TEntity>(Expression<Func<TEntity, bool>> condition, CancellationToken cancellationToken = default)
         where TEntity : BaseEntity;

        Task<TEntity> GetAsync<TEntity>(
     Expression<Func<TEntity, bool>> condition,
     bool asNoTracking,
     CancellationToken cancellationToken = default)
     where TEntity : BaseEntity;

        Task<TEntity> GetAsync<TEntity>(
        Expression<Func<TEntity, bool>> condition,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> includes,
        CancellationToken cancellationToken = default)
        where TEntity : BaseEntity;

        Task<TEntity> GetAsync<TEntity>(
           Expression<Func<TEntity, bool>> condition,
           Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> includes,
           bool asNoTracking,
           CancellationToken cancellationToken = default)
           where TEntity : BaseEntity;

        Task<TEntity> GetAsync<TEntity>(Specification<TEntity> specification, CancellationToken cancellationToken = default)
       where TEntity : BaseEntity;

        Task<TEntity> GetAsync<TEntity>(
         Specification<TEntity> specification,
         bool asNoTracking,
         CancellationToken cancellationToken = default)
         where TEntity : BaseEntity;

        Task<TProjectedType> GetAsync<TEntity, TProjectedType>(
        Expression<Func<TEntity, bool>> condition,
        Expression<Func<TEntity, TProjectedType>> selectExpression,
        CancellationToken cancellationToken = default)
        where TEntity : BaseEntity;

        Task<TProjectedType> GetAsync<TEntity, TProjectedType>(
       Specification<TEntity> specification,
       Expression<Func<TEntity, TProjectedType>> selectExpression,
       CancellationToken cancellationToken = default)
       where TEntity : BaseEntity;

        Task<bool> ExistsAsync<TEntity>(CancellationToken cancellationToken = default)
         where TEntity : BaseEntity;

        Task<bool> ExistsAsync<TEntity>(Expression<Func<TEntity, bool>> condition, CancellationToken cancellationToken = default)
          where TEntity : BaseEntity;

        Task<bool> ExistsByIdAsync<TEntity>(Guid? id, CancellationToken cancellationToken = default)
          where TEntity : BaseEntity;

        Task<int> GetCountAsync<TEntity>(CancellationToken cancellationToken = default)
          where TEntity : BaseEntity;

        Task<int> GetCountAsync<TEntity>(Expression<Func<TEntity, bool>> condition, CancellationToken cancellationToken = default)
       where TEntity : BaseEntity;

        Task<int> GetCountAsync<TEntity>(IEnumerable<Expression<Func<TEntity, bool>>> conditions, CancellationToken cancellationToken = default)
          where TEntity : BaseEntity;

        Task<long> GetLongCountAsync<TEntity>(CancellationToken cancellationToken = default)
         where TEntity : BaseEntity;

        Task<long> GetLongCountAsync<TEntity>(Expression<Func<TEntity, bool>> condition, CancellationToken cancellationToken = default)
       where TEntity : BaseEntity;

        Task<long> GetLongCountAsync<TEntity>(IEnumerable<Expression<Func<TEntity, bool>>> conditions, CancellationToken cancellationToken = default)
          where TEntity : BaseEntity;

        Task<List<T>> GetFromRawSqlAsync<T>(string sql, CancellationToken cancellationToken = default);

        Task<List<T>> GetFromRawSqlAsync<T>(string sql, object parameter, CancellationToken cancellationToken = default);

        Task<List<T>> GetFromRawSqlAsync<T>(
        string sql,
        IEnumerable<DbParameter> parameters,
        CancellationToken cancellationToken = default);

        Task<List<T>> GetFromRawSqlAsync<T>(string sql, IEnumerable<object> parameters, CancellationToken cancellationToken = default);
    }
}