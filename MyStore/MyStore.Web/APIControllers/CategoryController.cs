using BusinessLogic.Services.CategoryServices;
using BusinessLogic.Services.Product;
using BusinessLogic.Services.ProductImage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Model;
using Repository.ViewModels;

namespace MyStore.Web.APIControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;
        private readonly IProductImageService _productImageService;

        public CategoryController(ICategoryService categoryService, IProductService productService, IProductImageService productImageService)
        {
            _categoryService = categoryService;
            _productService = productService;
            _productImageService = productImageService;
        }

        // Lấy tất cả category
        [HttpGet("GetAllCategories")]
        public async Task<IActionResult> GetAllCategories()
        {
            try
            {
                var categories = await _categoryService.ListAsync();
                var result = categories.Select(c => new
                {
                    c.CategoryId,
                    c.Name,
                    c.Description,
                    c.IsActive
                });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Lấy tất cả category đang hoạt động
        [HttpGet("GetActiveCategories")]
        public async Task<IActionResult> GetActiveCategories()
        {
            try
            {
                var categories = await _categoryService.ListAsync(c => c.IsActive);
                var result = categories.Select(c => new
                {
                    c.CategoryId,
                    c.Name,
                    c.Description,
                    c.IsActive
                });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Tạo mới category
        [HttpPost("CreateCategory")]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryCreateUpdateViewModel model)
        {
            try
            {
                if (model == null || string.IsNullOrWhiteSpace(model.Name))
                    return BadRequest("Invalid data.");

                // Check trùng tên (không phân biệt hoa thường)
                var existed = await _categoryService.FindAsync(
                    c => c.Name.ToLower().Trim() == model.Name.Trim().ToLower()
                );
                if (existed != null)
                    return BadRequest("Category name already exists.");

                var category = new Categories
                {
                    CategoryId = Guid.NewGuid(),
                    Name = model.Name.Trim(),
                    Description = model.Description,
                    IsActive = true
                };

                await _categoryService.AddAsync(category);
                await _categoryService.SaveChangesAsync();

                return Ok(new { Message = "Category created successfully!", CategoryId = category.CategoryId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Cập nhật thông tin category
        [HttpPut("UpdateCategory/{id:guid}")]
        public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] CategoryCreateUpdateViewModel model)
        {
            try
            {
                var category = await _categoryService.FindAsync(c => c.CategoryId == id);
                if (category == null)
                    return NotFound("Category not found.");

                if (string.IsNullOrWhiteSpace(model.Name))
                    return BadRequest("Category name is required.");

                // Check trùng tên (không phân biệt hoa thường, loại trừ chính nó)
                var existed = await _categoryService.FindAsync(
                    c => c.Name.ToLower().Trim() == model.Name.Trim().ToLower() && c.CategoryId != id
                );
                if (existed != null)
                    return BadRequest("Category name already exists.");

                category.Name = model.Name.Trim();
                category.Description = model.Description;

                await _categoryService.UpdateAsync(category);
                await _categoryService.SaveChangesAsync();

                return Ok(new { Message = "Category updated successfully!", CategoryId = category.CategoryId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Cập nhật trạng thái IsActive (ẩn/hiện)
        [HttpPatch("SetActive/{id:guid}")]
        public async Task<IActionResult> SetActive(Guid id, [FromBody] bool isActive)
        {
            try
            {
                var category = await _categoryService.FindAsync(c => c.CategoryId == id);
                if (category == null)
                    return NotFound("Category not found.");

                category.IsActive = isActive;
                await _categoryService.UpdateAsync(category);
                await _categoryService.SaveChangesAsync();

                return Ok(new
                {
                    Message = "Category IsActive updated successfully!",
                    CategoryId = category.CategoryId,
                    IsActive = category.IsActive
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("GetProductsByCategory/{categoryId:guid}")]
        public async Task<IActionResult> GetProductsByCategory(Guid categoryId)
        {
            try
            {
                // Lấy sản phẩm theo categoryId, chỉ lấy sản phẩm đang hoạt động
                var products = await _productService.ListAsync(
                    filter: p => p.CategoryID == categoryId && p.IsActive,
                    orderBy: null,
                    includeProperties: q => q
                        .Include(p => p.ProductImages)
                        .Include(p => p.Category)
                );

                if (products == null || !products.Any())
                    return NotFound("No products found in this category.");

                var result = products.Select(p => new
                {
                    p.ProductId,
                    p.Name,
                    p.Description,
                    Price = Math.Round(p.Price, 0),
                    p.StockQuantity,
                    p.CreatedDate,
                    p.IsActive,
                    p.CategoryID,
                    CategoryName = p.Category != null ? p.Category.Name : null,
                    Images = p.ProductImages?.Select(img => img.ImageUrl).ToList()
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
