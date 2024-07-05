using AhmedOumezzine.EFCore.Repository.Entities;
using AhmedOumezzine.EFCore.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;
using System.Diagnostics;

namespace AhmedOumezzine.EFCore.Repository.Repository
{
    [DebuggerStepThrough]
    public sealed partial class Repository<TDbContext> : IRepository
        where TDbContext : DbContext
    {
        private readonly TDbContext _dbContext;

        public Repository(TDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync(
            IsolationLevel isolationLevel = IsolationLevel.Unspecified,
            CancellationToken cancellationToken = default)
        {
            IDbContextTransaction dbContextTransaction = await _dbContext.Database.BeginTransactionAsync(isolationLevel, cancellationToken);
            return dbContextTransaction;
        }

        public IQueryable<T> GetQueryable<T>() where T : BaseEntity
        {
            return _dbContext.Set<T>();
        }

        public void ClearChangeTracker()
        {
            _dbContext.ChangeTracker.Clear();
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            int count = await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return count;
        }
    }
}