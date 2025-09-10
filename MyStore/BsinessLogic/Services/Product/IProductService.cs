using System.Linq.Expressions;
using Model;

namespace BusinessLogic.Services.Product
{
    public interface IProductService
    {
        IQueryable<Products> GetAll();
        Products GetById(Guid id);
        Task<Products> GetAsyncById(Guid id);
        Products Find(Expression<Func<Products, bool>> match);
        Task<Products> FindAsync(Expression<Func<Products, bool>> match);
        Task AddAsync(Products entity);
        Task UpdateAsync(Products entity);
        Task DeleteAsync(Products entity);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        int Count();
        Task<int> CountAsync();
        Task<IEnumerable<Products>> ListAsync();
        Task<IEnumerable<Products>> ListAsync(
            Expression<Func<Products, bool>> filter = null,
            Func<IQueryable<Products>, IOrderedQueryable<Products>> orderBy = null,
            Func<IQueryable<Products>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Products, object>> includeProperties = null);
    }
}
