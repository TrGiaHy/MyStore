using Model;
using Model.DBContext;
using Repository.BaseRepository;
using Repository.OtherReposity.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.OtherReposity.OrderItem
{
    public class OrderItemRepository : BaseRepository<Model.OrderItems>, IOrderItemRepository
    {
        public OrderItemRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }
    }
}
