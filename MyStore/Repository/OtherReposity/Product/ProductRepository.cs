using Model.DBContext;
using Repository.BaseRepository;
using Repository.OtherReposity.OrderItem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.OtherReposity.Product
{
    public class ProductRepository : BaseRepository<Model.Products>, IProductRepository
    {
        public ProductRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }
    }
}
