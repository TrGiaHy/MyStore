using BusinessLogic.Services.Product;
using Microsoft.AspNetCore.Mvc;

namespace MyStore.Web.APIControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            var products = await _productService.ListAsync();
            return Ok(products);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProductsIsActive()
        {
            var products = await _productService.ListAsync(filter: p => p.IsActive);
            if (products == null || !products.Any())
            {
                return NotFound("No active products found.");
            }
            return Ok(products);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetProductById(Guid id)
        {
            var product = await _productService.GetAsyncById(id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchProductsByName([FromQuery] string name)
        {
            //if (string.IsNullOrWhiteSpace(name))
            //{
            //    return BadRequest("Product name is required.");
            //}

            var products = await _productService.ListAsync(
                filter: p => p.Name.Contains(name) && p.IsActive
            );
            if (products == null || !products.Any())
            {
                // Thông báo khi không tìm thấy sản phẩm
                //Console.WriteLine($"No active products found matching the name: {name}");
                return NotFound($"No active products found matching the name: {name}");
            }
            return Ok(products);
        }
    }
}
