using Model;
using Model.DBContext;
using Repository.BaseRepository;
using Repository.OtherReposity.ProductImage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.OtherReposity.ShoppingCart
{
    public class ShoppingCartRepository : BaseRepository<Model.ShoppingCarts>, IShoppingCartRepository
    {
        public ShoppingCartRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }
    }
}
