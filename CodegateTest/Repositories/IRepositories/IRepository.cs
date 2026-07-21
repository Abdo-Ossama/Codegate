using System.Linq.Expressions;

namespace CodegateTest.Repositories.IRepositories
{
    public interface IRepository<T> where T : class
    {
        Task CreateAsync(
            T entity,
            CancellationToken cancellationToken = default);

        void Update(T entity);

        void Delete(T entity);

        Task<int> CommitAsync(
            CancellationToken cancellationToken = default);

        Task<IEnumerable<T>> GetAsync(
            Expression<Func<T, bool>>? expression = null,
            Expression<Func<T, object>>?[]? includes = null,
            bool tracked = true,
            CancellationToken cancellationToken = default);

        Task<T?> GetOneAsync(
            Expression<Func<T, bool>>? expression = null,
            Expression<Func<T, object>>?[]? includes = null,
            bool tracked = true,
            CancellationToken cancellationToken = default);
    }
}