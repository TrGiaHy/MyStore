using System.Linq.Expressions;
using Model;
using Repository.OtherReposity.Order;

namespace BusinessLogic.Services.Order
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _repository;
        //private readonly IMapper _mapper;

        public OrderService(IOrderRepository cate)
        {
            _repository = cate;
            //_mapper = mapper;
        }
        public IQueryable<Orders> GetAll() => _repository.GetAll();

        public Orders GetById(Guid id) => _repository.GetById(id);

        public async Task<Orders> GetAsyncById(Guid id) => await _repository.GetAsyncById(id);

        public Orders Find(Expression<Func<Orders, bool>> match) => _repository.Find(match);

        public async Task<Orders> FindAsync(Expression<Func<Orders, bool>> match) => await _repository.FindAsync(match);

        public async Task AddAsync(Orders entity) => await _repository.AddAsync(entity);

        public async Task UpdateAsync(Orders entity) => await _repository.UpdateAsync(entity);

        public async Task DeleteAsync(Orders entity) => await _repository.DeleteAsync(entity);

        public async Task DeleteAsync(Guid id) => await _repository.DeleteAsync(id);

        public async Task<bool> ExistsAsync(Guid id) => await _repository.ExistsAsync(id);
        public async Task<int> SaveChangesAsync() => await _repository.SaveChangesAsync();
        public int Count() => _repository.Count();

        public async Task<int> CountAsync() => await _repository.CountAsync();

        public async Task<IEnumerable<Orders>> ListAsync() => await _repository.ListAsync();

        public async Task<IEnumerable<Orders>> ListAsync(
            Expression<Func<Orders, bool>> filter = null,
            Func<IQueryable<Orders>, IOrderedQueryable<Orders>> orderBy = null,
            Func<IQueryable<Orders>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Orders, object>> includeProperties = null) =>
            await _repository.ListAsync(filter, orderBy, includeProperties);
    }
}
