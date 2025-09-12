using System.Linq.Expressions;
using Model;
using Repository.OtherReposity.CartItem;

namespace BusinessLogic.Services.CartItem
{
    public class CartItemService : ICartItemService
    {
        private readonly ICartItemRepository _repository;
        //private readonly IMapper _mapper;

        public CartItemService(ICartItemRepository cate)
        {
            _repository = cate;
            //_mapper = mapper;
        }
        public IQueryable<CartItems> GetAll() => _repository.GetAll();

        public CartItems GetById(Guid id) => _repository.GetById(id);

        public async Task<CartItems> GetAsyncById(Guid id) => await _repository.GetAsyncById(id);

        public CartItems Find(Expression<Func<CartItems, bool>> match) => _repository.Find(match);

        public async Task<CartItems> FindAsync(Expression<Func<CartItems, bool>> match) => await _repository.FindAsync(match);

        public async Task AddAsync(CartItems entity) => await _repository.AddAsync(entity);

        public async Task UpdateAsync(CartItems entity) => await _repository.UpdateAsync(entity);

        public async Task DeleteAsync(CartItems entity) => await _repository.DeleteAsync(entity);

        public async Task DeleteAsync(Guid id) => await _repository.DeleteAsync(id);

        public async Task<bool> ExistsAsync(Guid id) => await _repository.ExistsAsync(id);
        public async Task<int> SaveChangesAsync() => await _repository.SaveChangesAsync();

        public int Count() => _repository.Count();

        public async Task<int> CountAsync() => await _repository.CountAsync();

        public async Task<IEnumerable<CartItems>> ListAsync() => await _repository.ListAsync();

        public async Task<IEnumerable<CartItems>> ListAsync(
            Expression<Func<CartItems, bool>> filter = null,
            Func<IQueryable<CartItems>, IOrderedQueryable<CartItems>> orderBy = null,
            Func<IQueryable<CartItems>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<CartItems, object>> includeProperties = null) =>
            await _repository.ListAsync(filter, orderBy, includeProperties);
    }
}
