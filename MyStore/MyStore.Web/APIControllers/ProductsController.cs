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
                // Lấy tất cả sản phẩm active, bao gồm cả hình ảnh
                var products = await _productService.ListAsync(
                    orderBy: null,
                    includeProperties: q => q
                        .Include(p => p.ProductImages)
                );

                if (products == null || !products.Any())
                {
                    return NotFound("No active products found.");
                }

                // Trả về danh sách sản phẩm kèm hình ảnh
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

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("GetAllProductsIsActive")]
        public async Task<IActionResult> GetAllProductsIsActive()
        {
            try
            {
                // Lấy tất cả sản phẩm active, bao gồm cả hình ảnh và category
                var products = await _productService.ListAsync(
                    filter: p => p.IsActive,
                    orderBy: null,
                    includeProperties: q => q
                        .Include(p => p.ProductImages)
                        .Include(p => p.Category) // Include thêm Category
                );

                if (products == null || !products.Any())
                {
                    return NotFound("No active products found.");
                }

                // Trả về danh sách sản phẩm kèm hình ảnh và tên category
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

        [HttpGet("GetProductById/{id:guid}")]
        public async Task<IActionResult> GetProductById(Guid id)
        {
            try
            {
                // Lấy sản phẩm kèm hình ảnh
                var products = await _productService.ListAsync(
                    filter: p => p.ProductId == id,
                    orderBy: null,
                    includeProperties: q => q.Include(p => p.ProductImages)
                );

                var product = products.FirstOrDefault();
                if (product == null)
                {
                    return NotFound();
                }

                var result = new
                {
                    product.ProductId,
                    product.Name,
                    product.Description,
                    product.Price,
                    product.StockQuantity,
                    product.CreatedDate,
                    product.IsActive,
                    product.CategoryID,
                    Images = product.ProductImages?.Select(img => img.ImageUrl).ToList()
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
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
                    return NotFound($"No active products found matching the name: {name}");
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

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("CreateProduct")]
        public async Task<IActionResult> CreateProduct([FromForm] CreateProductViewModel model)
        {
            if (model == null)
                return BadRequest("Invalid data.");

            if (string.IsNullOrWhiteSpace(model.Name))
                return BadRequest("Product name is required.");

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

                return Ok(new { Message = "Product created successfully!", ProductId = product.ProductId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("UpdateProduct/{id:guid}")]
        public async Task<IActionResult> UpdateProduct([FromRoute] Guid id, [FromForm] CreateProductViewModel model)
        {
            if (model == null)
                return BadRequest("Invalid data.");

            if (string.IsNullOrWhiteSpace(model.Name))
                return BadRequest("Product name is required.");

            try
            {
                // Lấy sản phẩm hiện tại
                var product = await _productService.FindAsync(p => p.ProductId == id);
                if (product == null)
                    return NotFound("Product not found.");

                // Cập nhật thông tin sản phẩm
                product.Name = model.Name;
                product.Description = model.Description;
                product.Price = model.Price;
                product.StockQuantity = model.StockQuantity;
                product.CategoryID = model.CategoryID;

                await _productService.UpdateAsync(product);
                await _productService.SaveChangesAsync();

                // Xử lý cập nhật hình ảnh (nếu có gửi lên)
                if (model.Images != null && model.Images.Any())
                {
                    // Xóa toàn bộ ảnh cũ
                    var oldImages = await _productImageService.ListAsync(img => img.ProductID == product.ProductId);
                    foreach (var oldImg in oldImages)
                    {
                        // Xóa file vật lý nếu tồn tại
                        var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", oldImg.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                        await _productImageService.DeleteAsync(oldImg);
                    }
                    await _productImageService.SaveChangesAsync();

                    // Lưu các ảnh mới
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

                return Ok(new { Message = "Product updated successfully!", ProductId = product.ProductId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPatch("HideProduct/{id:guid}")]
        public async Task<IActionResult> HideProduct(Guid id)
        {
            try
            {
                var product = await _productService.FindAsync(p => p.ProductId == id);
                if (product == null)
                    return NotFound("Product not found.");

                product.IsActive = false;
                product.StockQuantity = 0; // Set stock to 0 when hiding
                await _productService.UpdateAsync(product);
                await _productService.SaveChangesAsync();

                return Ok(new
                {
                    Message = "Product hide successfully!",
                    ProductId = product.ProductId,
                    IsActive = product.IsActive,
                    StockQuantity = product.StockQuantity
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPatch("ShowProduct/{id:guid}")]
        public async Task<IActionResult> ShowProduct(Guid id)
        {
            try
            {
                var product = await _productService.FindAsync(p => p.ProductId == id);
                if (product == null)
                    return NotFound("Product not found.");

                product.IsActive = true;
                await _productService.UpdateAsync(product);
                await _productService.SaveChangesAsync();

                return Ok(new { Message = "Product show successfully!", ProductId = product.ProductId, IsActive = product.IsActive });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
