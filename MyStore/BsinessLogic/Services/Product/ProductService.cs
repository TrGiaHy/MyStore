using System.Linq.Expressions;
using Model;
using Repository.OtherReposity.Product;

namespace BusinessLogic.Services.Product
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;
        //private readonly IMapper _mapper;

        public ProductService(IProductRepository cate)
        {
            _repository = cate;
            //_mapper = mapper;
        }
        public IQueryable<Products> GetAll() => _repository.GetAll();

        public Products GetById(Guid id) => _repository.GetById(id);

        public async Task<Products> GetAsyncById(Guid id) => await _repository.GetAsyncById(id);

        public Products Find(Expression<Func<Products, bool>> match) => _repository.Find(match);

        public async Task<Products> FindAsync(Expression<Func<Products, bool>> match) => await _repository.FindAsync(match);

        public async Task AddAsync(Products entity) => await _repository.AddAsync(entity);

        public async Task UpdateAsync(Products entity) => await _repository.UpdateAsync(entity);

        public async Task DeleteAsync(Products entity) => await _repository.DeleteAsync(entity);

        public async Task DeleteAsync(Guid id) => await _repository.DeleteAsync(id);

        public async Task<bool> ExistsAsync(Guid id) => await _repository.ExistsAsync(id);
        public async Task<int> SaveChangesAsync() => await _repository.SaveChangesAsync();
        public int Count() => _repository.Count();

        public async Task<int> CountAsync() => await _repository.CountAsync();

        public async Task<IEnumerable<Products>> ListAsync() => await _repository.ListAsync();

        public async Task<IEnumerable<Products>> ListAsync(
            Expression<Func<Products, bool>> filter = null,
            Func<IQueryable<Products>, IOrderedQueryable<Products>> orderBy = null,
            Func<IQueryable<Products>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Products, object>> includeProperties = null) =>
            await _repository.ListAsync(filter, orderBy, includeProperties);
    }
}
