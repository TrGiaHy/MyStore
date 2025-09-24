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

        public class CheckoutClientModel
        {
            public string ShippingAddress { get; set; }
            public string PaymentMethod { get; set; }
            public List<Guid> ProductIds { get; set; }
        }

        [HttpPost("Customer/Checkout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout([FromBody] CheckoutClientModel client)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Json(new { success = false, message = "Please login before checkout." });

            if (client == null || client.ProductIds == null || !client.ProductIds.Any())
                return Json(new { success = false, message = "No products selected for checkout." });

            if (string.IsNullOrWhiteSpace(client.ShippingAddress))
                return Json(new { success = false, message = "Shipping address is required." });

            if (string.IsNullOrWhiteSpace(client.PaymentMethod))
                return Json(new { success = false, message = "Payment method is required." });

            var payload = new CheckoutViewModel
            {
                UserId = user.Id,
                ShippingAddress = client.ShippingAddress,
                PaymentMethod = client.PaymentMethod,
                ProductIds = client.ProductIds
            };

            var apiResponse = await _apiClient.PostJsonAsync<object>("/api/Cart/Checkout", payload);

            if (apiResponse == null)
                return Json(new { success = false, message = "No response from API." });

            if (!apiResponse.Success)
                return Json(new { success = false, message = apiResponse.ErrorMessage });

            return Json(new
            {
                success = true,
                message = "Checkout successful.",
                redirectUrl = Url.Action("GetOrderHistoryByUser", "Customer")
            });
        }


        [HttpGet("Customer/GetOrderHistoryByUser")]
        public async Task<IActionResult> GetOrderHistoryByUser(string id)
        {
            // Kiểm tra người dùng có đăng nhập hay không
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Home");
            }
            var apiResponse = await _apiClient.GetJsonAsync<OrderHistoryResponse>(
                $"/api/Orders/GetOrderHistoryByUser/{user.Id}"
            );

            var orders = (apiResponse.Success && apiResponse.Data?.Orders != null)
                ? apiResponse.Data.Orders
                : new List<OrderSummaryViewModel>();

            return View("GetOrderHistoryByUser", orders);
        }

        [HttpGet("Customer/GetOrderProducts")]
        public async Task<IActionResult> GetOrderProducts([FromQuery] Guid orderId)
        {
            if (orderId == Guid.Empty)
                return Json(new { success = false, message = "Invalid order id." });

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Json(new { success = false, message = "Please login." });

            var apiResponse = await _apiClient.GetJsonAsync<OrderProductsResponse>(
                $"/api/Orders/GetOrderProducts/{orderId}"
            );

            if (apiResponse == null || !apiResponse.Success || apiResponse.Data == null)
                return Json(new { success = false, message = apiResponse?.ErrorMessage ?? "Failed to load order products." });

            return Json(new { success = true, data = apiResponse.Data });
        }

        public class CancelOrderInput
        {
            public Guid OrderId { get; set; }
        }

        [HttpPost("Customer/CancelOrder")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder([FromBody] CancelOrderInput input)
        {
            if (input == null || input.OrderId == Guid.Empty)
                return Json(new { success = false, message = "Invalid order id." });

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Json(new { success = false, message = "Please login." });

            try
            {
                // API returns { Message, OrderId, Status } (not ApiResponse<T>), PatchAsync will deserialize to object on 2xx
                var apiResult = await _apiClient.PatchAsync<object>($"/api/Orders/CancelOrder/{input.OrderId}", new { });
                if (apiResult == null)
                    return Json(new { success = false, message = "No response from API." });

                return Json(new { success = true, message = "Order canceled successfully." });
            }
            catch (Exception ex)
            {
                // ApiClientService throws with "API error (StatusCode): content" on non-2xx
                var msg = ex.Message
                    .Replace("API error (BadRequest):", "", StringComparison.OrdinalIgnoreCase)
                    .Replace("API error (Unauthorized):", "", StringComparison.OrdinalIgnoreCase)
                    .Replace("API error (Forbidden):", "", StringComparison.OrdinalIgnoreCase)
                    .Replace("API error (NotFound):", "", StringComparison.OrdinalIgnoreCase)
                    .Trim();
                return Json(new { success = false, message = msg });
            }
        }
    }
}
