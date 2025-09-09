using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Model;

namespace BsinessLogic.Services.ShoppingCart
{
    public interface IShoppingCartService
    {
        IQueryable<ShoppingCarts> GetAll();
        ShoppingCarts GetById(Guid id);
        Task<ShoppingCarts> GetAsyncById(Guid id);
        ShoppingCarts Find(Expression<Func<ShoppingCarts, bool>> match);
        Task<ShoppingCarts> FindAsync(Expression<Func<ShoppingCarts, bool>> match);
        Task AddAsync(ShoppingCarts entity);
        Task UpdateAsync(ShoppingCarts entity);
        Task DeleteAsync(ShoppingCarts entity);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        int Count();
        Task<int> CountAsync();
        Task<IEnumerable<ShoppingCarts>> ListAsync();
        Task<IEnumerable<ShoppingCarts>> ListAsync(
            Expression<Func<ShoppingCarts, bool>> filter = null,
            Func<IQueryable<ShoppingCarts>, IOrderedQueryable<ShoppingCarts>> orderBy = null,
            Func<IQueryable<ShoppingCarts>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<ShoppingCarts, object>> includeProperties = null);
    }
}
