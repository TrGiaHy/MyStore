using System.Linq.Expressions;
using Model;

namespace BusinessLogic.Services.Order
{
    public interface IOrderService
    {
        IQueryable<Orders> GetAll();
        Orders GetById(Guid id);
        Task<Orders> GetAsyncById(Guid id);
        Orders Find(Expression<Func<Orders, bool>> match);
        Task<Orders> FindAsync(Expression<Func<Orders, bool>> match);
        Task AddAsync(Orders entity);
        Task UpdateAsync(Orders entity);
        Task DeleteAsync(Orders entity);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<int> SaveChangesAsync();
        int Count();
        Task<int> CountAsync();
        Task<IEnumerable<Orders>> ListAsync();
        Task<IEnumerable<Orders>> ListAsync(
            Expression<Func<Orders, bool>> filter = null,
            Func<IQueryable<Orders>, IOrderedQueryable<Orders>> orderBy = null,
            Func<IQueryable<Orders>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Orders, object>> includeProperties = null);
    }
}
