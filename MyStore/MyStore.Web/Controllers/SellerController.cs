using BusinessLogic.Services.ApiClientService;
using Microsoft.AspNetCore.Mvc;
using Repository.ViewModels;

namespace MyStore.Web.Controllers
{
    public class SellerController : Controller
    {
        private readonly IApiClientService _apiClient;

        public SellerController(IApiClientService apiClient)
        {
            _apiClient = apiClient;
        }

        // GET: /Seller/Products
        [HttpGet]
        public async Task<IActionResult> ManageProducts()
        {
            // Call API: GET /api/Products/GetAllProducts
            var resp = await _apiClient.GetJsonAsync<List<ProductViewModel>>("/api/Products/GetAllProducts");
            var products = resp.Success && resp.Data != null ? resp.Data : new List<ProductViewModel>();
            if (!resp.Success) ViewBag.ErrorMessage = resp.ErrorMessage ?? "Failed to load products.";
            return View(products);
        }

        public class ToggleActiveInput { public Guid ProductId { get; set; } public bool IsActive { get; set; } }

        [HttpPost("Seller/SetActive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetActive([FromBody] ToggleActiveInput input)
        {
            if (input == null || input.ProductId == Guid.Empty)
                return Json(new { success = false, message = "Invalid product id." });

            try
            {
                var url = input.IsActive
                    ? $"/api/Products/ShowProduct/{input.ProductId}"
                    : $"/api/Products/HideProduct/{input.ProductId}";

                var result = await _apiClient.PatchAsync<object>(url, new { });
                if (result == null) return Json(new { success = false, message = "No response from API." });

                return Json(new { success = true, message = input.IsActive ? "Product shown." : "Product hidden." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
