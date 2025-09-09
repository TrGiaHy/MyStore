using AutoMapper;
using Model;
using Repository.OtherReposity.OrderItem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BsinessLogic.Services.OrderItem
{
    public class OrderItemService : IOrderItemService
    {
        private readonly IOrderItemRepository _repository;
        private readonly IMapper _mapper;

        public OrderItemService(IOrderItemRepository cate, IMapper mapper)
        {
            _repository = cate;
            _mapper = mapper;
        }
        public IQueryable<OrderItems> GetAll() => _repository.GetAll();

        public OrderItems GetById(Guid id) => _repository.GetById(id);

        public async Task<OrderItems> GetAsyncById(Guid id) => await _repository.GetAsyncById(id);

        public OrderItems Find(Expression<Func<OrderItems, bool>> match) => _repository.Find(match);

        public async Task<OrderItems> FindAsync(Expression<Func<OrderItems, bool>> match) => await _repository.FindAsync(match);

        public async Task AddAsync(OrderItems entity) => await _repository.AddAsync(entity);

        public async Task UpdateAsync(OrderItems entity) => await _repository.UpdateAsync(entity);

        public async Task DeleteAsync(OrderItems entity) => await _repository.DeleteAsync(entity);

        public async Task DeleteAsync(Guid id) => await _repository.DeleteAsync(id);

        public async Task<bool> ExistsAsync(Guid id) => await _repository.ExistsAsync(id);

        public int Count() => _repository.Count();

        public async Task<int> CountAsync() => await _repository.CountAsync();

        public async Task<IEnumerable<OrderItems>> ListAsync() => await _repository.ListAsync();

        public async Task<IEnumerable<OrderItems>> ListAsync(
            Expression<Func<OrderItems, bool>> filter = null,
            Func<IQueryable<OrderItems>, IOrderedQueryable<OrderItems>> orderBy = null,
            Func<IQueryable<OrderItems>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<OrderItems, object>> includeProperties = null) =>
            await _repository.ListAsync(filter, orderBy, includeProperties);
    }
}
