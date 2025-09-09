using BsinessLogic.Services.CartItem;
using BsinessLogic.Services.Category;
using BsinessLogic.Services.CategoryServices;
using BsinessLogic.Services.Order;
using BsinessLogic.Services.OrderItem;
using BsinessLogic.Services.Product;
using BsinessLogic.Services.ProductImage;
using BsinessLogic.Services.ShoppingCart;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BsinessLogic.Config
{
    public static class ConfigServices
    {
        public static void ConfigureServices(this IServiceCollection services)
        {
            services.AddScoped<ICartItemService, CartItemService>();

            services.AddScoped<ICategoryService, CategoryService>();

            services.AddScoped<IOrderService, OrderService>();

            services.AddScoped<IOrderItemService, OrderItemService>();

            services.AddScoped<IProductService, ProductService>();

            services.AddScoped<IProductImageService, ProductImageService>();

            services.AddScoped<IShoppingCartService, ShoppingCartService>();

          
        }
    }
}
