using System.Linq.Expressions;
using Model;
using Repository.OtherReposity.ProductImage;

namespace BusinessLogic.Services.ProductImage
{
    public class ProductImageService : IProductImageService
    {
        private readonly IProductImageRepository _repository;
        //private readonly IMapper _mapper;

        public ProductImageService(IProductImageRepository cate)
        {
            _repository = cate;
            //_mapper = mapper;
        }
        public IQueryable<ProductImages> GetAll() => _repository.GetAll();

        public ProductImages GetById(Guid id) => _repository.GetById(id);

        public async Task<ProductImages> GetAsyncById(Guid id) => await _repository.GetAsyncById(id);

        public ProductImages Find(Expression<Func<ProductImages, bool>> match) => _repository.Find(match);

        public async Task<ProductImages> FindAsync(Expression<Func<ProductImages, bool>> match) => await _repository.FindAsync(match);

        public async Task AddAsync(ProductImages entity) => await _repository.AddAsync(entity);

        public async Task UpdateAsync(ProductImages entity) => await _repository.UpdateAsync(entity);

        public async Task DeleteAsync(ProductImages entity) => await _repository.DeleteAsync(entity);

        public async Task DeleteAsync(Guid id) => await _repository.DeleteAsync(id);

        public async Task<bool> ExistsAsync(Guid id) => await _repository.ExistsAsync(id);

        public int Count() => _repository.Count();

        public async Task<int> CountAsync() => await _repository.CountAsync();

        public async Task<IEnumerable<ProductImages>> ListAsync() => await _repository.ListAsync();

        public async Task<IEnumerable<ProductImages>> ListAsync(
            Expression<Func<ProductImages, bool>> filter = null,
            Func<IQueryable<ProductImages>, IOrderedQueryable<ProductImages>> orderBy = null,
            Func<IQueryable<ProductImages>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<ProductImages, object>> includeProperties = null) =>
            await _repository.ListAsync(filter, orderBy, includeProperties);
    }
}
