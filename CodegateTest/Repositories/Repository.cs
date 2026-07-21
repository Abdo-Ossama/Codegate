using CodegateTest.DataAccess;
using CodegateTest.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq.Expressions;

namespace CodegateTest.Repositories
{

    public class Repository<T> : IRepository<T>
     where T : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;
        private readonly ILogger<Repository<T>> _logger;

        public Repository(
            ApplicationDbContext context,
            ILogger<Repository<T>> logger)
        {
            _context = context;
            _dbSet = _context.Set<T>();
            _logger = logger;
        }

        public async Task CreateAsync(T entity , CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(entity, cancellationToken);
         
        }
        public void Update(T entity )
        {
            _dbSet.Update(entity);
        }
        public void Delete(T entity )
        {
            _dbSet.Remove(entity);
        }


        public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                //Console.WriteLine($"Error: {ex.Message}");
                return 0;
            }
        }

        //getAll
        public async Task<IEnumerable<T>> GetAsync(
     Expression<Func<T, bool>>? expression = null,
     Expression<Func<T, object>>?[]? includes = null,
     bool tracked = true,
     CancellationToken cancellationToken = default)
        {
          var values = _dbSet.AsQueryable();

            if (expression is not null)
            {
                values = values.Where(expression);
            }

            if (includes is not null)
            {
                foreach (var item in includes)
                {
                    if (item is not null)
                    {
                        values = values.Include(item);
                    }
                }
            }

            if (!tracked)
            {
                values = values.AsNoTracking();
            }

            return await values.ToListAsync(cancellationToken);
        }


        public async Task <T?> GetOneAsync(Expression<Func<T, bool>>? expression,
            Expression<Func<T, Object>>?[] includes,
            bool tracked = true , 
            CancellationToken cancellationToken = default 
            )
        {
            return(await GetAsync(expression , includes , tracked , cancellationToken)).FirstOrDefault();
        }

    }
}
