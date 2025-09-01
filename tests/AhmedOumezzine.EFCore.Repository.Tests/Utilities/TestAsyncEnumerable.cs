
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace AhmedOumezzine.EFCore.Repository.Tests.Utilities
{
    public class TestAsyncEnumerable<T> : IQueryable<T>, IAsyncEnumerable<T>
    {
        private readonly Expression _expression;
        private readonly IAsyncQueryProvider _provider;
        private readonly IEnumerable<T> _items;

        // ✅ Constructeur 1 : à partir d'une liste
        public TestAsyncEnumerable(IEnumerable<T> items)
        {
            _items = items ?? new List<T>();
            _expression = _items.AsQueryable().Expression;
            _provider = new TestAsyncQueryProvider<T>(_items.AsQueryable().Provider);
        }

        // ✅ Constructeur 2 : à partir d'une expression et d'un provider
        public TestAsyncEnumerable(Expression expression, IAsyncQueryProvider provider)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            _expression = expression;
            _provider = provider;
            _items = new List<T>(); // placeholder — sera évalué via le provider
        }

        // ✅ Implémentation de IQueryable<T>
        public Type ElementType => typeof(T);
        public Expression Expression => _expression;
        public IQueryProvider Provider => _provider;

        public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();

        // ✅ Implémentation de IAsyncEnumerable<T>
        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new TestAsyncEnumerator<T>(_items.GetEnumerator());
        }
    }
}
