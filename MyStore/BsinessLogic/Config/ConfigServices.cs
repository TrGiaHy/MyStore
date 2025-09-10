using BusinessLogic.Services.CartItem;
using BusinessLogic.Services.Category;
using BusinessLogic.Services.CategoryServices;
using BusinessLogic.Services.Order;
using BusinessLogic.Services.OrderItem;
using BusinessLogic.Services.Product;
using BusinessLogic.Services.ProductImage;
using BusinessLogic.Services.ShoppingCart;
using Microsoft.Extensions.DependencyInjection;

namespace BusinessLogic.Config
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
