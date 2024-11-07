using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using WebProject_API_React.Server.Repository.IRepository;
using WebProject_API_React.Server.Data;

namespace WebProject_API_React.Server.Repository
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private readonly ApplicationDbContext _context;
        internal DbSet<TEntity> dbSet;
        public Repository(ApplicationDbContext db)
        {
            _context = db;
            this.dbSet = _context.Set<TEntity>();
        }
        public async Task AddAsync(TEntity entity)
        {
            dbSet.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(TEntity entity)
        {
            dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(TEntity entity)
        {
            dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            IQueryable<TEntity> query = dbSet;
            return await query.ToListAsync(cancellationToken);
        }

        public async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
        {
            IQueryable<TEntity> query = dbSet;
            query = query.Where(filter);
            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
        {
            IQueryable<TEntity> query = dbSet;
            query = query.Where(filter);
            return await query.ToListAsync(cancellationToken);
        }


    }
}
