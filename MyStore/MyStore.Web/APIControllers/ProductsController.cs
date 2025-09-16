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
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IProductImageService _productImageService;

        public ProductsController(IProductService productService, IProductImageService productImageService)
        {
            _productService = productService;
            _productImageService = productImageService;
        }

        [HttpGet("GetAllProducts")]
        public async Task<IActionResult> GetAllProducts()
        {
            try
            {
                var products = await _productService.ListAsync(
                    orderBy: null,
                    includeProperties: q => q.Include(p => p.ProductImages)
                );

                if (products == null || !products.Any())
                {
                    return Ok(new ApiResponse<string>
                    {
                        Success = false,
                        ErrorMessage = "No products found.",
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

        [HttpGet("GetAllProductsIsActive")]
        public async Task<IActionResult> GetAllProductsIsActive()
        {
            try
            {
                var products = await _productService.ListAsync(
                    filter: p => p.IsActive,
                    orderBy: null,
                    includeProperties: q => q.Include(p => p.ProductImages)
                                              .Include(p => p.Category)
                );

                if (products == null || !products.Any())
                {
                    return Ok(new ApiResponse<string>
                    {
                        Success = false,
                        ErrorMessage = "No active products found.",
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

        [HttpGet("GetProductById/{id:guid}")]
        public async Task<IActionResult> GetProductById(Guid id)
        {
            try
            {
                var products = await _productService.ListAsync(
                    filter: p => p.ProductId == id,
                    orderBy: null,
                    includeProperties: q => q.Include(p => p.ProductImages)
                                             .Include(p => p.Category)
                );

                var product = products.FirstOrDefault();
                if (product == null)
                {
                    return Ok(new ApiResponse<string>
                    {
                        Success = false,
                        ErrorMessage = "Product not found.",
                        StatusCode = 404
                    });
                }

                var result = new
                {
                    product.ProductId,
                    product.Name,
                    product.Description,
                    Price = Math.Round(product.Price, 0),
                    product.StockQuantity,
                    product.CreatedDate,
                    product.IsActive,
                    product.CategoryID,
                    CategoryName = product.Category != null ? product.Category.Name : null,
                    Images = product.ProductImages?.Select(img => img.ImageUrl).ToList()
                };

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


        [HttpGet("SearchProductsByName")]
        public async Task<IActionResult> SearchProductsByName([FromQuery] string name)
        {
            try
            {
                var products = await _productService.ListAsync(
                    filter: p => p.Name.Contains(name) && p.IsActive,
                    orderBy: null,
                    includeProperties: q => q.Include(p => p.ProductImages)
                );

                if (products == null || !products.Any())
                {
                    return Ok(new ApiResponse<string>
                    {
                        Success = false,
                        ErrorMessage = $"No active products found matching the name: {name}",
                        StatusCode = 404
                    });
                }

                var result = products.Select(p => new
                {
                    p.ProductId,
                    p.Name,
                    p.Description,
                    p.Price,
                    p.StockQuantity,
                    p.CreatedDate,
                    p.IsActive,
                    p.CategoryID,
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

        [HttpPost("CreateProduct")]
        public async Task<IActionResult> CreateProduct([FromForm] CreateProductViewModel model)
        {
            if (model == null)
            {
                return Ok(new ApiResponse<string>
                {
                    Success = false,
                    ErrorMessage = "Invalid data.",
                    StatusCode = 400
                });
            }

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                return Ok(new ApiResponse<string>
                {
                    Success = false,
                    ErrorMessage = "Product name is required.",
                    StatusCode = 400
                });
            }

            if (model.Price < 1000)
            {
                return Ok(new ApiResponse<string>
                {
                    Success = false,
                    ErrorMessage = "Product price must be at least 1000.",
                    StatusCode = 400
                });
            }

            try
            {
                var product = new Products
                {
                    ProductId = Guid.NewGuid(),
                    Name = model.Name,
                    Description = model.Description,
                    Price = model.Price,
                    StockQuantity = model.StockQuantity,
                    CreatedDate = DateTime.Now,
                    IsActive = true,
                    CategoryID = model.CategoryID
                };

                await _productService.AddAsync(product);
                await _productService.SaveChangesAsync();

                var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadFolder))
                    Directory.CreateDirectory(uploadFolder);

                if (model.Images != null && model.Images.Any())
                {
                    foreach (var image in model.Images)
                    {
                        if (image != null && image.Length > 0)
                        {
                            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                            var filePath = Path.Combine(uploadFolder, fileName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await image.CopyToAsync(stream);
                            }

                            var productImage = new ProductImages
                            {
                                ProductImageID = Guid.NewGuid(),
                                ProductID = product.ProductId,
                                ImageUrl = $"/uploads/{fileName}"
                            };

                            await _productImageService.AddAsync(productImage);
                        }
                    }
                    await _productImageService.SaveChangesAsync();
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = new { Message = "Product created successfully!", ProductId = product.ProductId },
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

        [HttpPut("UpdateProduct/{id:guid}")]
        public async Task<IActionResult> UpdateProduct([FromRoute] Guid id, [FromForm] CreateProductViewModel model)
        {
            if (model == null)
            {
                return Ok(new ApiResponse<string>
                {
                    Success = false,
                    ErrorMessage = "Invalid data.",
                    StatusCode = 400
                });
            }

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                return Ok(new ApiResponse<string>
                {
                    Success = false,
                    ErrorMessage = "Product name is required.",
                    StatusCode = 400
                });
            }

            if (model.Price < 1000)
            {
                return Ok(new ApiResponse<string>
                {
                    Success = false,
                    ErrorMessage = "Product price must be at least 1000.",
                    StatusCode = 400
                });
            }

            try
            {
                var product = await _productService.FindAsync(p => p.ProductId == id);
                if (product == null)
                {
                    return Ok(new ApiResponse<string>
                    {
                        Success = false,
                        ErrorMessage = "Product not found.",
                        StatusCode = 404
                    });
                }

                product.Name = model.Name;
                product.Description = model.Description;
                product.Price = model.Price;
                product.StockQuantity = model.StockQuantity;
                product.CategoryID = model.CategoryID;

                await _productService.UpdateAsync(product);
                await _productService.SaveChangesAsync();

                if (model.Images != null && model.Images.Any())
                {
                    var oldImages = await _productImageService.ListAsync(img => img.ProductID == product.ProductId);
                    foreach (var oldImg in oldImages)
                    {
                        var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", oldImg.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                        await _productImageService.DeleteAsync(oldImg);
                    }
                    await _productImageService.SaveChangesAsync();

                    var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    if (!Directory.Exists(uploadFolder))
                        Directory.CreateDirectory(uploadFolder);

                    foreach (var image in model.Images)
                    {
                        if (image != null && image.Length > 0)
                        {
                            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                            var filePath = Path.Combine(uploadFolder, fileName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await image.CopyToAsync(stream);
                            }

                            var productImage = new ProductImages
                            {
                                ProductImageID = Guid.NewGuid(),
                                ProductID = product.ProductId,
                                ImageUrl = $"/uploads/{fileName}"
                            };

                            await _productImageService.AddAsync(productImage);
                        }
                    }
                    await _productImageService.SaveChangesAsync();
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = new { Message = "Product updated successfully!", ProductId = product.ProductId },
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

        [HttpPatch("HideProduct/{id:guid}")]
        public async Task<IActionResult> HideProduct(Guid id)
        {
            try
            {
                var product = await _productService.FindAsync(p => p.ProductId == id);
                if (product == null)
                {
                    return Ok(new ApiResponse<string>
                    {
                        Success = false,
                        ErrorMessage = "Product not found.",
                        StatusCode = 404
                    });
                }

                product.IsActive = false;
                product.StockQuantity = 0;
                await _productService.UpdateAsync(product);
                await _productService.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = new
                    {
                        Message = "Product hide successfully!",
                        ProductId = product.ProductId,
                        IsActive = product.IsActive,
                        StockQuantity = product.StockQuantity
                    },
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

        [HttpPatch("ShowProduct/{id:guid}")]
        public async Task<IActionResult> ShowProduct(Guid id)
        {
            try
            {
                var product = await _productService.FindAsync(p => p.ProductId == id);
                if (product == null)
                {
                    return Ok(new ApiResponse<string>
                    {
                        Success = false,
                        ErrorMessage = "Product not found.",
                        StatusCode = 404
                    });
                }

                product.IsActive = true;
                await _productService.UpdateAsync(product);
                await _productService.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = new { Message = "Product show successfully!", ProductId = product.ProductId, IsActive = product.IsActive },
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
