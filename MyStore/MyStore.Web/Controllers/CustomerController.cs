using BusinessLogic.Services.ApiClientService;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Model;
using Repository.ViewModels;

namespace MyStore.Web.Controllers
{

    public class CustomerController : Controller
    {
        private readonly IApiClientService _apiClient;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;

        public CustomerController(IApiClientService apiClient, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _apiClient = apiClient;
            _userManager = userManager;
            _signInManager = signInManager;
        }
        //Home/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Lấy categories
            var categoriesResponse = await _apiClient.GetJsonAsync<List<CategoryViewModel>>("/api/Category/GetActiveCategories");
            ViewBag.Categories = categoriesResponse.Success ? categoriesResponse.Data : new List<CategoryViewModel>();

            // Lấy sản phẩm active
            var productsResponse = await _apiClient.GetJsonAsync<List<ProductViewModel>>("/api/Products/GetAllProductsIsActive");

            List<ProductViewModel> products = new List<ProductViewModel>();

            if (productsResponse.Success && productsResponse.Data != null)
            {
                products = productsResponse.Data;
            }
            else
            {
                // Có thể hiển thị thông báo lỗi trong View
                ViewBag.ErrorMessage = productsResponse.ErrorMessage ?? "No products found.";
            }

            return View(products);
        }

        //load lại sản phẩm theo category khi click vào category ở Home/Index
        [HttpGet]
        public async Task<IActionResult> ProductGrid(Guid? categoryId)
        {
            string apiUrl = categoryId.HasValue
                ? $"/api/Category/GetProductsByCategory/{categoryId.Value}"
                : "/api/Products/GetAllProductsIsActive";

            var response = await _apiClient.GetJsonAsync<List<ProductViewModel>>(apiUrl);

            var products = response.Success && response.Data != null
                ? response.Data
                : new List<ProductViewModel>();

            return PartialView("_ProductGrid", products);
        }

        [HttpGet("Customer/ProductDetail/{id:guid}")]
        public async Task<IActionResult> ProductDetail(Guid id)
        {
            // Log id để kiểm tra
            Console.WriteLine($"ProductDetail called with id: {id}");

            // Lấy categories
            var categoriesResponse = await _apiClient.GetJsonAsync<List<CategoryViewModel>>("/api/Category/GetActiveCategories");
            ViewBag.Categories = categoriesResponse.Success && categoriesResponse.Data != null
                ? categoriesResponse.Data
                : new List<CategoryViewModel>();

            // Lấy chi tiết sản phẩm
            var productResponse = await _apiClient.GetJsonAsync<ProductViewModel>($"/api/Products/GetProductById/{id}");
            if (!productResponse.Success || productResponse.Data == null)
            {
                return NotFound("Sản phẩm không tồn tại hoặc đã bị xóa.");
            }
            var product = productResponse.Data;

            // Lấy sản phẩm liên quan cùng category, loại trừ sản phẩm hiện tại
            var relatedResponse = await _apiClient.GetJsonAsync<List<ProductViewModel>>(
                $"/api/Category/GetProductsByCategory/{product.CategoryID}"
            );

            var relatedProducts = relatedResponse.Success && relatedResponse.Data != null
                ? relatedResponse.Data.Where(p => p.ProductId != id).ToList()
                : new List<ProductViewModel>();

            ViewBag.RelatedProducts = relatedProducts;

            return View(product);
        }

        [HttpGet("Customer/GetCartItemsByUser")]
        public async Task<IActionResult> GetCartItemsByUser()
        {
            try
            {
                // Lấy user đang đăng nhập
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return RedirectToAction("Login", "Authentication");

                // Gọi API lấy giỏ hàng
                var cartResponse = await _apiClient.GetJsonAsync<List<CartItemViewModel>>(
                    $"/api/Cart/GetCartItemsByUser?userId={user.Id}"
                );

                var cartItems = cartResponse.Success && cartResponse.Data != null
                    ? cartResponse.Data
                    : new List<CartItemViewModel>();

                return View(cartItems);
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View(new List<CartItemViewModel>());
            }
        }


        [HttpPost]
        public async Task<IActionResult> AddToCart(Guid productId, int quantity)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Authentication");

            var addToCartRequest = new AddToCartViewModel
            {
                UserID = user.Id,
                ProductId = productId,
                Quantity = quantity
            };

            var apiResponse = await _apiClient.PostJsonAsync<object>("api/cart/AddToCart", addToCartRequest);

            if (!apiResponse.Success)
            {
                // Trả về JSON lỗi để xử lý Ajax
                return Json(new { success = false, message = apiResponse.ErrorMessage });
            }

            return Json(new
            {
                success = true,
                message = "Product added to cart successfully!"
            });
        }

        [HttpGet("Customer/Checkout")]
        public async Task<IActionResult> Checkout()
        {
            try
            {
                // Lấy user đang đăng nhập
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return RedirectToAction("Login", "Authentication");

                // Lấy giỏ hàng để hiển thị tất cả sản phẩm
                var cartResponse = await _apiClient.GetJsonAsync<List<CartItemViewModel>>(
                    $"/api/Cart/GetCartItemsByUser?userId={user.Id}"
                );

                var cartItems = cartResponse.Success && cartResponse.Data != null
                    ? cartResponse.Data
                    : new List<CartItemViewModel>();

                ViewBag.UserId = user.Id;
                return View(cartItems);
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View(new List<CartItemViewModel>());
            }
        }

        [HttpPost("Customer/PlaceOrder")]
        public async Task<IActionResult> PlaceOrder([FromBody] CheckoutViewModel request)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return Json(new { success = false, message = "User not authenticated" });

                request.UserId = user.Id;

                var apiResponse = await _apiClient.PostJsonAsync<object>("api/Cart/Checkout", request);

                if (!apiResponse.Success)
                {
                    return Json(new { success = false, message = apiResponse.ErrorMessage });
                }

                return Json(new { success = true, message = "Order placed successfully!", data = apiResponse.Data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

    }
}
