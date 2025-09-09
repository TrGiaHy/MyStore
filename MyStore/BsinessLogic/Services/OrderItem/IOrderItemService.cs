using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Model;

namespace BsinessLogic.Services.OrderItem
{
    public interface IOrderItemService
    {
        IQueryable<OrderItems> GetAll();
        OrderItems GetById(Guid id);
        Task<OrderItems> GetAsyncById(Guid id);
        OrderItems Find(Expression<Func<OrderItems, bool>> match);
        Task<OrderItems> FindAsync(Expression<Func<OrderItems, bool>> match);
        Task AddAsync(OrderItems entity);
        Task UpdateAsync(OrderItems entity);
        Task DeleteAsync(OrderItems entity);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        int Count();
        Task<int> CountAsync();
        Task<IEnumerable<OrderItems>> ListAsync();
        Task<IEnumerable<OrderItems>> ListAsync(
            Expression<Func<OrderItems, bool>> filter = null,
            Func<IQueryable<OrderItems>, IOrderedQueryable<OrderItems>> orderBy = null,
            Func<IQueryable<OrderItems>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<OrderItems, object>> includeProperties = null);
    }
}
