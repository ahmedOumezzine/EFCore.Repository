using AhmedOumezzine.EFCore.Repository.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AhmedOumezzine.EFCore.Repository.Tests
{
    // BaseRepositoryTests.cs
    public abstract class BaseRepositoryTests
    {
        protected Mock<TestDbContext> _mockContext;
        protected Mock<DbSet<T>> CreateMockDbSet<T>(IEnumerable<T> data) where T : class
        {
            var queryable = data.AsQueryable();
            var mockSet = new Mock<DbSet<T>>();

            var asyncQueryProvider = new TestAsyncQueryProvider<T>(queryable.Provider);

            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(asyncQueryProvider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            mockSet.As<IAsyncEnumerable<T>>().Setup(m => m.GetAsyncEnumerator(default))
                   .Returns(new TestAsyncEnumerator<T>(queryable.GetEnumerator()));

            return mockSet;
        }

        protected void SetupDbContext<TEntity>(Mock<DbSet<TEntity>> mockSet)
            where TEntity : class
        {
            _mockContext = new Mock<TestDbContext>();
            _mockContext.Setup(c => c.Set<TEntity>()).Returns(mockSet.Object);
            _mockContext.Setup(c => c.SaveChanges()).Verifiable();
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                        .ReturnsAsync(1)
                        .Verifiable();
        }
    }
}
