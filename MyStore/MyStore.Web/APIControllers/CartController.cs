using BusinessLogic.Services.CartItem;
using BusinessLogic.Services.Product;
using BusinessLogic.Services.ShoppingCart;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Model;
using Repository.ViewModels;
using static BusinessLogic.Services.ApiClientService.ApiClientService;

namespace MyStore.Web.APIControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IProductService _productService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ICartItemService _cartItemService;

        public CartController(UserManager<AppUser> userManager, IProductService productService, IShoppingCartService shoppingCartService, ICartItemService cartItemService)
        {
            _userManager = userManager;
            _productService = productService;
            _shoppingCartService = shoppingCartService;
            _cartItemService = cartItemService;
        }

        [HttpPost("AddToCart")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartViewModel request)
        {
            if (request == null)
            {
                return Ok(new ApiResponse<string>
                {
                    Success = false,
                    ErrorMessage = "Request data is required.",
                    StatusCode = 400
                });
            }

            if (request.Quantity <= 0)
            {
                return Ok(new ApiResponse<string>
                {
                    Success = false,
                    ErrorMessage = "Quantity must be greater than 0.",
                    StatusCode = 400
                });
            }

            if (string.IsNullOrWhiteSpace(request.UserID))
            {
                return Ok(new ApiResponse<string>
                {
                    Success = false,
                    ErrorMessage = "UserID is required.",
                    StatusCode = 400
                });
            }

            var product = await _productService.FindAsync(
                p => p.ProductId == request.ProductId && p.IsActive
            );

            if (product == null)
            {
                return Ok(new ApiResponse<string>
                {
                    Success = false,
                    ErrorMessage = "Product not found or inactive.",
                    StatusCode = 404
                });
            }

            try
            {
                // Lấy cart của user
                var cart = (await _shoppingCartService.ListAsync(
                    filter: c => c.UserID == request.UserID,
                    includeProperties: q => q.Include(c => c.CartItems)
                )).FirstOrDefault();

                if (cart == null)
                {
                    cart = new ShoppingCarts
                    {
                        ID = Guid.NewGuid(),
                        UserID = request.UserID,
                        CartItems = new List<CartItems>()
                    };
                    await _shoppingCartService.AddAsync(cart);
                    await _shoppingCartService.SaveChangesAsync();
                }

                // Kiểm tra sản phẩm trong giỏ
                var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == request.ProductId);
                int newQuantity = cartItem?.Quantity + request.Quantity ?? request.Quantity;

                // Kiểm tra tồn kho
                if (newQuantity > product.StockQuantity)
                {
                    return Ok(new ApiResponse<string>
                    {
                        Success = false,
                        ErrorMessage = $"Cannot add to cart. Total quantity {newQuantity} exceeds product stock {product.StockQuantity}.",
                        StatusCode = 400
                    });
                }

                if (cartItem != null)
                {
                    cartItem.Quantity = newQuantity;
                    await _cartItemService.UpdateAsync(cartItem);
                }
                else
                {
                    await _cartItemService.AddAsync(new CartItems
                    {
                        CartItemId = Guid.NewGuid(),
                        CartId = cart.ID,
                        ProductId = request.ProductId,
                        Quantity = request.Quantity
                    });
                }

                await _shoppingCartService.SaveChangesAsync();

                var totalItems = (await _cartItemService.ListAsync(ci => ci.CartId == cart.ID))
                    .Sum(ci => ci.Quantity);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = new
                    {
                        Message = "Product added to cart successfully.",
                        CartId = cart.ID,
                        TotalItems = totalItems
                    },
                    StatusCode = 200,
                    Username = request.UserID
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

        [HttpDelete("RemoveFromCart")]
        public async Task<IActionResult> RemoveFromCart([FromBody] RemoveFromCartViewModel request)
        {
            if (request == null)
                return Ok(new ApiResponse<string> { Success = false, ErrorMessage = "Request data is required.", StatusCode = 400 });

            if (string.IsNullOrWhiteSpace(request.UserID))
                return Ok(new ApiResponse<string> { Success = false, ErrorMessage = "UserID is required.", StatusCode = 400 });

            try
            {
                var cart = (await _shoppingCartService.ListAsync(
                    filter: c => c.UserID == request.UserID,
                    includeProperties: q => q.Include(c => c.CartItems)
                )).FirstOrDefault();

                if (cart == null)
                    return Ok(new ApiResponse<string> { Success = false, ErrorMessage = "Cart not found for this user.", StatusCode = 404 });

                var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == request.ProductId);
                if (cartItem == null)
                    return Ok(new ApiResponse<string> { Success = false, ErrorMessage = "Product not found in cart.", StatusCode = 404 });

                await _cartItemService.DeleteAsync(cartItem);
                cart.CartItems.Remove(cartItem);

                var totalItems = (await _cartItemService.ListAsync(ci => ci.CartId == cart.ID)).Sum(ci => ci.Quantity);

                await _shoppingCartService.UpdateAsync(cart);
                await _shoppingCartService.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = new { Message = "Product removed from cart successfully.", CartId = cart.ID, TotalItems = totalItems },
                    StatusCode = 200,
                    Username = request.UserID
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

        [HttpGet("GetCartItemsByUser")]
        public async Task<IActionResult> GetCartItemsByUser([FromQuery] string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return Ok(new ApiResponse<string> { Success = false, ErrorMessage = "UserID is required.", StatusCode = 400 });

            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return Ok(new ApiResponse<string> { Success = false, ErrorMessage = "User does not exist.", StatusCode = 404 });

                var cart = (await _shoppingCartService.ListAsync(
                    filter: c => c.UserID == userId,
                    includeProperties: q => q.Include(c => c.CartItems).ThenInclude(ci => ci.Product)
                )).FirstOrDefault();

                if (cart == null)
                    return Ok(new ApiResponse<string> { Success = false, ErrorMessage = "Cart not found for this user.", StatusCode = 404 });

                if (cart.CartItems == null || !cart.CartItems.Any())
                    return Ok(new ApiResponse<string> { Success = false, ErrorMessage = "Cart does not contain any items.", StatusCode = 404 });

                var result = cart.CartItems
                    .Where(ci => ci.Product != null && ci.Product.IsActive)
                    .Select(ci => new
                    {
                        ci.ProductId,
                        ProductName = ci.Product.Name,
                        ProductDescription = ci.Product.Description,
                        ProductPrice = Math.Round(ci.Product.Price, 0, MidpointRounding.AwayFromZero),
                        ci.Quantity
                    })
                    .ToList();

                if (!result.Any())
                    return Ok(new ApiResponse<string> { Success = false, ErrorMessage = "All products in cart are deleted or inactive.", StatusCode = 404 });

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = result,
                    StatusCode = 200,
                    Username = userId
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

        [HttpPut("UpdateCartItemQuantity")]
        public async Task<IActionResult> UpdateCartItemQuantity([FromBody] UpdateCartItemQuantityViewModel request)
        {
            if (request == null)
                return Ok(new ApiResponse<string> { Success = false, ErrorMessage = "Request data is required.", StatusCode = 400 });

            if (string.IsNullOrWhiteSpace(request.UserID))
                return Ok(new ApiResponse<string> { Success = false, ErrorMessage = "UserID is required.", StatusCode = 400 });

            if (request.Quantity < 0)
                return Ok(new ApiResponse<string> { Success = false, ErrorMessage = "Quantity must be >= 0.", StatusCode = 400 });

            try
            {
                var user = await _userManager.FindByIdAsync(request.UserID);
                if (user == null)
                    return Ok(new ApiResponse<string> { Success = false, ErrorMessage = "User does not exist.", StatusCode = 404 });

                var product = await _productService.FindAsync(p => p.ProductId == request.ProductId && p.IsActive);
                if (product == null)
                    return Ok(new ApiResponse<string> { Success = false, ErrorMessage = "Product not found or inactive.", StatusCode = 404 });

                var cart = (await _shoppingCartService.ListAsync(
                    filter: c => c.UserID == request.UserID,
                    includeProperties: q => q.Include(c => c.CartItems)
                )).FirstOrDefault();

                if (cart == null)
                    return Ok(new ApiResponse<string> { Success = false, ErrorMessage = "Cart not found for this user.", StatusCode = 404 });

                var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == request.ProductId);
                if (cartItem == null)
                    return Ok(new ApiResponse<string> { Success = false, ErrorMessage = "Product not found in cart.", StatusCode = 404 });

                if (request.Quantity > product.StockQuantity)
                    return Ok(new ApiResponse<string> { Success = false, ErrorMessage = $"Cannot set quantity greater than product stock ({product.StockQuantity}).", StatusCode = 400 });

                if (request.Quantity == 0)
                {
                    await _cartItemService.DeleteAsync(cartItem);
                    cart.CartItems.Remove(cartItem);
                }
                else
                {
                    cartItem.Quantity = request.Quantity;
                    await _cartItemService.UpdateAsync(cartItem);
                }

                await _shoppingCartService.UpdateAsync(cart);
                await _shoppingCartService.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = new { Message = "Cart item updated successfully.", CartId = cart.ID, ProductId = request.ProductId, Quantity = request.Quantity },
                    StatusCode = 200,
                    Username = request.UserID
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

        [HttpPost("Checkout")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutViewModel request)
        {
            if (request == null)
                return Ok(new ApiResponse<string> { Success = false, ErrorMessage = "Request data is required.", StatusCode = 400 });

            if (string.IsNullOrWhiteSpace(request.UserId))
                return Ok(new ApiResponse<string> { Success = false, ErrorMessage = "UserID is required.", StatusCode = 400 });

            if (string.IsNullOrWhiteSpace(request.ShippingAddress))
                return Ok(new ApiResponse<string> { Success = false, ErrorMessage = "Shipping address is required.", StatusCode = 400 });

            if (string.IsNullOrWhiteSpace(request.PaymentMethod))
                return Ok(new ApiResponse<string> { Success = false, ErrorMessage = "Payment method is required.", StatusCode = 400 });

            if (request.ProductIds == null || !request.ProductIds.Any())
                return Ok(new ApiResponse<string> { Success = false, ErrorMessage = "No products selected for checkout.", StatusCode = 400 });

            try
            {
                var user = await _userManager.FindByIdAsync(request.UserId);
                if (user == null)
                    return Ok(new ApiResponse<string> { Success = false, ErrorMessage = "User does not exist.", StatusCode = 404 });

                var cart = (await _shoppingCartService.ListAsync(
                    filter: c => c.UserID == request.UserId,
                    includeProperties: q => q.Include(c => c.CartItems).ThenInclude(ci => ci.Product)
                )).FirstOrDefault();

                if (cart == null || cart.CartItems == null || !cart.CartItems.Any())
                    return Ok(new ApiResponse<string> { Success = false, ErrorMessage = "Cart is empty.", StatusCode = 400 });

                var selectedCartItems = cart.CartItems.Where(ci => request.ProductIds.Contains(ci.ProductId)).ToList();
                if (!selectedCartItems.Any())
                    return Ok(new ApiResponse<string> { Success = false, ErrorMessage = "Selected products are not in the cart.", StatusCode = 400 });

                foreach (var item in selectedCartItems)
                {
                    if (item.Quantity > item.Product.StockQuantity)
                    {
                        return Ok(new ApiResponse<string>
                        {
                            Success = false,
                            ErrorMessage = $"Product '{item.Product.Name}' does not have enough stock. Available: {item.Product.StockQuantity}, Requested: {item.Quantity}",
                            StatusCode = 400
                        });
                    }
                }

                var order = new Orders
                {
                    OrderId = Guid.NewGuid(),
                    UserId = request.UserId,
                    OrderDate = DateTime.UtcNow,
                    ShippingAddress = request.ShippingAddress,
                    PaymentMethod = request.PaymentMethod,
                    Status = "Pending",
                    OrderItems = new List<OrderItems>()
                };

                foreach (var item in selectedCartItems)
                {
                    item.Product.StockQuantity -= item.Quantity;
                    await _productService.UpdateAsync(item.Product);

                    order.OrderItems.Add(new OrderItems
                    {
                        OrderItemId = Guid.NewGuid(),
                        OrderId = order.OrderId,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.Product.Price
                    });
                }

                var orderService = HttpContext.RequestServices.GetService(typeof(BusinessLogic.Services.Order.IOrderService)) as BusinessLogic.Services.Order.IOrderService;
                if (orderService == null)
                    return Ok(new ApiResponse<string> { Success = false, ErrorMessage = "Order service not available.", StatusCode = 500 });

                await orderService.AddAsync(order);
                await orderService.SaveChangesAsync();

                foreach (var item in selectedCartItems)
                    await _cartItemService.DeleteAsync(item);

                await _shoppingCartService.UpdateAsync(cart);
                await _shoppingCartService.SaveChangesAsync();
                await _productService.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = new { Message = "Checkout successful.", OrderId = order.OrderId, OrderDate = order.OrderDate },
                    StatusCode = 200,
                    Username = request.UserId
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

