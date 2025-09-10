using System.Linq.Expressions;
using BusinessLogic.Services.CategoryServices;
using Model;
using Repository.OtherReposity.Category;

namespace BusinessLogic.Services.Category
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repository;
        //private readonly IMapper _mapper;

        public CategoryService(ICategoryRepository cate)
        {
            _repository = cate;
            //_mapper = mapper;
        }
        public IQueryable<Categories> GetAll() => _repository.GetAll();

        public Categories GetById(Guid id) => _repository.GetById(id);

        public async Task<Categories> GetAsyncById(Guid id) => await _repository.GetAsyncById(id);

        public Categories Find(Expression<Func<Categories, bool>> match) => _repository.Find(match);

        public async Task<Categories> FindAsync(Expression<Func<Categories, bool>> match) => await _repository.FindAsync(match);

        public async Task AddAsync(Categories entity) => await _repository.AddAsync(entity);

        public async Task UpdateAsync(Categories entity) => await _repository.UpdateAsync(entity);

        public async Task DeleteAsync(Categories entity) => await _repository.DeleteAsync(entity);

        public async Task DeleteAsync(Guid id) => await _repository.DeleteAsync(id);

        public async Task<bool> ExistsAsync(Guid id) => await _repository.ExistsAsync(id);

        public int Count() => _repository.Count();

        public async Task<int> CountAsync() => await _repository.CountAsync();

        public async Task<IEnumerable<Categories>> ListAsync() => await _repository.ListAsync();

        public async Task<IEnumerable<Categories>> ListAsync(
            Expression<Func<Categories, bool>> filter = null,
            Func<IQueryable<Categories>, IOrderedQueryable<Categories>> orderBy = null,
            Func<IQueryable<Categories>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Categories, object>> includeProperties = null) =>
            await _repository.ListAsync(filter, orderBy, includeProperties);
    }
}
