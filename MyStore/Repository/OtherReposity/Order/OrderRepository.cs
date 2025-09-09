using Model.DBContext;
using Repository.BaseRepository;
using Repository.OtherReposity.Category;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.OtherReposity.Order
{
    public class OrderRepository : BaseRepository<Model.Orders>, IOrderRepository
    {
        public OrderRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }
    }
}
