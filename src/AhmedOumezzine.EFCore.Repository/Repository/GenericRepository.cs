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

        private void CheckEntityIsNull<TEntity>(TEntity entity)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
        }

        private void CheckEntitiesIsNull<TEntity>(IEnumerable<TEntity> entities)
        {
            if (entities is null)
            {
                throw new ArgumentNullException(nameof(entities));
            }
        }

        // Méthode pour définir CreatedOnUtc pour une seule entité
        private void SetCreatedOnUtc(BaseEntity entity)
        {
            entity.CreatedOnUtc = DateTime.UtcNow;
        }

        // Méthode pour définir CreatedOnUtc pour une liste d'entités
        private void SetCreatedOnUtc(IEnumerable<BaseEntity> entities)
        {
            foreach (var entity in entities)
            {
                SetCreatedOnUtc(entity);
            }
        }

        // Méthode pour définir LastModifiedOnUtc pour une seule entité
        public void SetLastModifiedOnUtc(BaseEntity entity)
        {
            entity.LastModifiedOnUtc = DateTime.UtcNow;
        }

        // Méthode pour définir LastModifiedOnUtc pour une liste d'entités
        public void SetLastModifiedOnUtc(IEnumerable<BaseEntity> entities)
        {
            foreach (var entity in entities)
            {
                SetLastModifiedOnUtc(entity);
            }
        } // Méthode pour définir IsDeleted sur true et DeletedOnUtc pour une seule entité

        public void SetDeleted(BaseEntity entity)
        {
            entity.IsDeleted = true;
            entity.DeletedOnUtc = DateTime.UtcNow;
        }

        // Méthode pour définir IsDeleted sur true et DeletedOnUtc pour une liste d'entités
        public void SetDeleted(IEnumerable<BaseEntity> entities)
        {
            foreach (var entity in entities)
            {
                SetDeleted(entity);
            }
        }
    }
}