using BusinessLogic.Services.ApiClientService;
using Microsoft.AspNetCore.Mvc;
using Repository.ViewModels;

namespace MyStore.Web.Controllers
{
    public class CustomerController : Controller
    {
        private readonly IApiClientService _apiClient;
        public CustomerController(IApiClientService apiClient)
        {
            _apiClient = apiClient;
        }
        //Home/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var categories = await _apiClient.GetAsync<List<CategoryViewModel>>("/api/Category/GetActiveCategories");
            ViewBag.Categories = categories;

            // Lấy tất cả sản phẩm active khi vào trang
            var products = await _apiClient.GetAsync<List<ProductViewModel>>("/api/Products/GetAllProductsIsActive");
            return View(products);
        }
        //load lại sản phẩm theo category khi click vào category ở Home/Index
        [HttpGet]
        public async Task<IActionResult> ProductGrid(Guid? categoryId)
        {
            string apiUrl = categoryId.HasValue
                ? $"/api/Category/GetProductsByCategory/{categoryId.Value}"
                : "/api/Products/GetAllProductsIsActive";

            var products = await _apiClient.GetAsync<List<ProductViewModel>>(apiUrl);
            return PartialView("_ProductGrid", products);
        }


    }
}
