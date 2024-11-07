using System.Linq.Expressions;

namespace WebProject_API_React.Server.Repository.IRepository
{
    public interface IRepository<TEntity> where TEntity : class
    {
        Task AddAsync(TEntity entity);
        Task UpdateAsync(TEntity entity);
        Task DeleteAsync(TEntity entity);
        Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default);
        Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default);
    }
}
