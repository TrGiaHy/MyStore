using System.Linq.Expressions;
using Model;

namespace BusinessLogic.Services.ProductImage
{
    public interface IProductImageService
    {
        IQueryable<ProductImages> GetAll();
        ProductImages GetById(Guid id);
        Task<ProductImages> GetAsyncById(Guid id);
        ProductImages Find(Expression<Func<ProductImages, bool>> match);
        Task<ProductImages> FindAsync(Expression<Func<ProductImages, bool>> match);
        Task AddAsync(ProductImages entity);
        Task UpdateAsync(ProductImages entity);
        Task DeleteAsync(ProductImages entity);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<int> SaveChangesAsync();
        int Count();
        Task<int> CountAsync();
        Task<IEnumerable<ProductImages>> ListAsync();
        Task<IEnumerable<ProductImages>> ListAsync(
            Expression<Func<ProductImages, bool>> filter = null,
            Func<IQueryable<ProductImages>, IOrderedQueryable<ProductImages>> orderBy = null,
            Func<IQueryable<ProductImages>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<ProductImages, object>> includeProperties = null);
    }
}
