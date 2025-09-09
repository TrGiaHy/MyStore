using Microsoft.EntityFrameworkCore;
using Model.DBContext;
using Repository.BaseRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.OtherReposity.CartItem
{
    public class CartItemRepository : BaseRepository<Model.CartItems>, ICartItemRepository
    {
        public CartItemRepository(ApplicationDbContext dbContext) : base(dbContext) { }
    }
}
