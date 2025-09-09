using Model;
using Model.DBContext;
using Repository.BaseRepository;
using Repository.OtherReposity.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.OtherReposity.ProductImage
{
    public class ProductImageRepository : BaseRepository<Model.ProductImages>, IProductImageRepository
    {
        public ProductImageRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }
    }
}
