using AhmedOumezzine.EFCore.Repository.Entities;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace AhmedOumezzine.EFCore.Repository.Interface
{
    public interface IRepository : IQueryRepository
    {
        Task<IDbContextTransaction> BeginTransactionAsync(
       IsolationLevel isolationLevel = IsolationLevel.Unspecified,
       CancellationToken cancellationToken = default);

        Task<object[]> InsertAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
         where TEntity : BaseEntity;

        Task InsertAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        [Obsolete("The method will be removed in the next version. Please use Update() and SaveChangesAsync() methods together.")]
        Task<int> UpdateAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
      where TEntity : BaseEntity;

        Task<int> UpdateAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        where TEntity : BaseEntity;

        Task<int> DeleteAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
          where TEntity : BaseEntity;

        Task<int> DeleteAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        where TEntity : BaseEntity;

        Task<int> ExecuteSqlCommandAsync(string sql, CancellationToken cancellationToken = default);

        Task<int> ExecuteSqlCommandAsync(string sql, params object[] parameters);

        Task<int> ExecuteSqlCommandAsync(string sql, IEnumerable<object> parameters, CancellationToken cancellationToken = default);

        void ClearChangeTracker();

        void Add<TEntity>(TEntity entity)
    where TEntity : BaseEntity;

        Task AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
     where TEntity : BaseEntity;

        void Add<TEntity>(IEnumerable<TEntity> entities)
       where TEntity : BaseEntity;

        Task AddAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
       where TEntity : BaseEntity;

        void Update<TEntity>(TEntity entity)
     where TEntity : BaseEntity;

        void Update<TEntity>(IEnumerable<TEntity> entities)
         where TEntity : BaseEntity;

        void Remove<TEntity>(TEntity entity)
    where TEntity : BaseEntity;

        void Remove<TEntity>(IEnumerable<TEntity> entities)
       where TEntity : BaseEntity;

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}