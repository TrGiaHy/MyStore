using System.Linq.Expressions;
using Model;

namespace BusinessLogic.Services.CartItem
{
    public interface ICartItemService
    {
        IQueryable<CartItems> GetAll();
        CartItems GetById(Guid id);
        Task<CartItems> GetAsyncById(Guid id);
        CartItems Find(Expression<Func<CartItems, bool>> match);
        Task<CartItems> FindAsync(Expression<Func<CartItems, bool>> match);
        Task AddAsync(CartItems entity);
        Task UpdateAsync(CartItems entity);
        Task DeleteAsync(CartItems entity);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<int> SaveChangesAsync();
        int Count();
        Task<int> CountAsync();
        Task<IEnumerable<CartItems>> ListAsync();
        Task<IEnumerable<CartItems>> ListAsync(
            Expression<Func<CartItems, bool>> filter = null,
            Func<IQueryable<CartItems>, IOrderedQueryable<CartItems>> orderBy = null,
            Func<IQueryable<CartItems>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<CartItems, object>> includeProperties = null);
    }
}
