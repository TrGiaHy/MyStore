using AutoMapper;
using Model;
using Repository.OtherReposity.ShoppingCart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BsinessLogic.Services.ShoppingCart
{
    public class ShoppingCartService : IShoppingCartService
    {
        private readonly IShoppingCartRepository _repository;
        private readonly IMapper _mapper;

        public ShoppingCartService(IShoppingCartRepository cate, IMapper mapper)
        {
            _repository = cate;
            _mapper = mapper;
        }
        public IQueryable<ShoppingCarts> GetAll() => _repository.GetAll();

        public ShoppingCarts GetById(Guid id) => _repository.GetById(id);

        public async Task<ShoppingCarts> GetAsyncById(Guid id) => await _repository.GetAsyncById(id);

        public ShoppingCarts Find(Expression<Func<ShoppingCarts, bool>> match) => _repository.Find(match);

        public async Task<ShoppingCarts> FindAsync(Expression<Func<ShoppingCarts, bool>> match) => await _repository.FindAsync(match);

        public async Task AddAsync(ShoppingCarts entity) => await _repository.AddAsync(entity);

        public async Task UpdateAsync(ShoppingCarts entity) => await _repository.UpdateAsync(entity);

        public async Task DeleteAsync(ShoppingCarts entity) => await _repository.DeleteAsync(entity);

        public async Task DeleteAsync(Guid id) => await _repository.DeleteAsync(id);

        public async Task<bool> ExistsAsync(Guid id) => await _repository.ExistsAsync(id);

        public int Count() => _repository.Count();

        public async Task<int> CountAsync() => await _repository.CountAsync();

        public async Task<IEnumerable<ShoppingCarts>> ListAsync() => await _repository.ListAsync();

        public async Task<IEnumerable<ShoppingCarts>> ListAsync(
            Expression<Func<ShoppingCarts, bool>> filter = null,
            Func<IQueryable<ShoppingCarts>, IOrderedQueryable<ShoppingCarts>> orderBy = null,
            Func<IQueryable<ShoppingCarts>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<ShoppingCarts, object>> includeProperties = null) =>
            await _repository.ListAsync(filter, orderBy, includeProperties);
    }
}
