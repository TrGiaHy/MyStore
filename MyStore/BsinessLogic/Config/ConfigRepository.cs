using Microsoft.Extensions.DependencyInjection;
using Model;
using Repository.OtherReposity.CartItem;
using Repository.OtherReposity.Category;
using Repository.OtherReposity.Order;
using Repository.OtherReposity.OrderItem;
using Repository.OtherReposity.Product;
using Repository.OtherReposity.ProductImage;
using Repository.OtherReposity.ShoppingCart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace BsinessLogic.Config
{
    public static class ConfigRepository
    {

        public static void ConfigureRepository(this IServiceCollection services)
        {
            services.AddScoped<ICartItemRepository, CartItemRepository>();

            services.AddScoped<ICategoryRepository, CategoryRepository>();

            services.AddScoped<IOrderRepository, OrderRepository>();

            services.AddScoped<IOrderItemRepository, OrderItemRepository>();

            services.AddScoped<IProductRepository, ProductRepository>();

            services.AddScoped<IProductImageRepository, ProductImageRepository>();

            services.AddScoped<IShoppingCartRepository, ShoppingCartRepository>();


        }
    }
}
