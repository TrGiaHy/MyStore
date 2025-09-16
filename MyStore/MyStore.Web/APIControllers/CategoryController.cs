using BusinessLogic.Services.CategoryServices;
using BusinessLogic.Services.Product;
using BusinessLogic.Services.ProductImage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Model;
using Repository.ViewModels;
using static BusinessLogic.Services.ApiClientService.ApiClientService;

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

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = result,
                    StatusCode = 200
                });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string>
                {
                    Success = false,
                    ErrorMessage = $"Internal server error: {ex.Message}",
                    StatusCode = 500
                });
            }
        }

        [HttpGet("GetActiveCategories")]
        public async Task<IActionResult> GetActiveCategories()
        {
            try
            {
                var categories = await _categoryService.ListAsync(c => c.IsActive);
                var products = await _productService.ListAsync(p => p.IsActive);

                var result = categories.Select(c => new
                {
                    c.CategoryId,
                    c.Name,
                    c.Description,
                    c.IsActive,
                    ProductCount = products.Count(p => p.CategoryID == c.CategoryId)
                });

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = result,
                    StatusCode = 200
                });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string>
                {
                    Success = false,
                    ErrorMessage = $"Internal server error: {ex.Message}",
                    StatusCode = 500
                });
            }
        }

        [HttpPost("CreateCategory")]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryCreateUpdateViewModel model)
        {
            try
            {
                if (model == null || string.IsNullOrWhiteSpace(model.Name))
                {
                    return Ok(new ApiResponse<string>
                    {
                        Success = false,
                        ErrorMessage = "Invalid data.",
                        StatusCode = 400
                    });
                }

                var existed = await _categoryService.FindAsync(
                    c => c.Name.ToLower().Trim() == model.Name.Trim().ToLower()
                );

                if (existed != null)
                {
                    return Ok(new ApiResponse<string>
                    {
                        Success = false,
                        ErrorMessage = "Category name already exists.",
                        StatusCode = 400
                    });
                }

                var category = new Categories
                {
                    CategoryId = Guid.NewGuid(),
                    Name = model.Name.Trim(),
                    Description = model.Description,
                    IsActive = true
                };

                await _categoryService.AddAsync(category);
                await _categoryService.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = new { Message = "Category created successfully!", CategoryId = category.CategoryId },
                    StatusCode = 200
                });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string>
                {
                    Success = false,
                    ErrorMessage = $"Internal server error: {ex.Message}",
                    StatusCode = 500
                });
            }
        }

        [HttpPut("UpdateCategory/{id:guid}")]
        public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] CategoryCreateUpdateViewModel model)
        {
            try
            {
                var category = await _categoryService.FindAsync(c => c.CategoryId == id);
                if (category == null)
                {
                    return Ok(new ApiResponse<string>
                    {
                        Success = false,
                        ErrorMessage = "Category not found.",
                        StatusCode = 404
                    });
                }

                if (string.IsNullOrWhiteSpace(model.Name))
                {
                    return Ok(new ApiResponse<string>
                    {
                        Success = false,
                        ErrorMessage = "Category name is required.",
                        StatusCode = 400
                    });
                }

                var existed = await _categoryService.FindAsync(
                    c => c.Name.ToLower().Trim() == model.Name.Trim().ToLower() && c.CategoryId != id
                );
                if (existed != null)
                {
                    return Ok(new ApiResponse<string>
                    {
                        Success = false,
                        ErrorMessage = "Category name already exists.",
                        StatusCode = 400
                    });
                }

                category.Name = model.Name.Trim();
                category.Description = model.Description;

                await _categoryService.UpdateAsync(category);
                await _categoryService.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = new { Message = "Category updated successfully!", CategoryId = category.CategoryId },
                    StatusCode = 200
                });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string>
                {
                    Success = false,
                    ErrorMessage = $"Internal server error: {ex.Message}",
                    StatusCode = 500
                });
            }
        }

        [HttpPatch("SetActive/{id:guid}")]
        public async Task<IActionResult> SetActive(Guid id, [FromBody] bool isActive)
        {
            try
            {
                var category = await _categoryService.FindAsync(c => c.CategoryId == id);
                if (category == null)
                {
                    return Ok(new ApiResponse<string>
                    {
                        Success = false,
                        ErrorMessage = "Category not found.",
                        StatusCode = 404
                    });
                }

                category.IsActive = isActive;
                await _categoryService.UpdateAsync(category);
                await _categoryService.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = new { Message = "Category IsActive updated successfully!", CategoryId = category.CategoryId, IsActive = category.IsActive },
                    StatusCode = 200
                });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string>
                {
                    Success = false,
                    ErrorMessage = $"Internal server error: {ex.Message}",
                    StatusCode = 500
                });
            }
        }

        [HttpGet("GetProductsByCategory/{categoryId:guid}")]
        public async Task<IActionResult> GetProductsByCategory(Guid categoryId)
        {
            try
            {
                var products = await _productService.ListAsync(
                    filter: p => p.CategoryID == categoryId && p.IsActive,
                    orderBy: null,
                    includeProperties: q => q
                        .Include(p => p.ProductImages)
                        .Include(p => p.Category)
                );

                if (products == null || !products.Any())
                {
                    return Ok(new ApiResponse<string>
                    {
                        Success = false,
                        ErrorMessage = "No products found in this category.",
                        StatusCode = 404
                    });
                }

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

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = result,
                    StatusCode = 200
                });
            }
            catch (Exception ex)
            {
                return Ok(new ApiResponse<string>
                {
                    Success = false,
                    ErrorMessage = $"Internal server error: {ex.Message}",
                    StatusCode = 500
                });
            }
        }

    }
}
