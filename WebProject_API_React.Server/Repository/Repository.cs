using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using WebProject.Server.Repository.IRepository;
using WebProject.Server.Data;

namespace WebProject.Server.Repository
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

        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            IQueryable<TEntity> query = dbSet;
            return query.ToList();
        }

        public async Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter)
        {
            IQueryable<TEntity> query = dbSet;
            query = query.Where(filter);
            return query.FirstOrDefault();
        }


    }
}
