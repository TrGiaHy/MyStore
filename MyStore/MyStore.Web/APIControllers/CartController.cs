using BusinessLogic.Services.CartItem;
using BusinessLogic.Services.Product;
using BusinessLogic.Services.ShoppingCart;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Model;
using Repository.ViewModels;

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
                return BadRequest("Request data is required.");
            }

            if (request.Quantity <= 0)
            {
                return BadRequest("Quantity must be greater than 0.");
            }

            if (string.IsNullOrWhiteSpace(request.UserID))
            {
                return BadRequest("UserID is required.");
            }

            // Kiểm tra sản phẩm tồn tại và còn active
            var product = await _productService.FindAsync(
                p => p.ProductId == request.ProductId && p.IsActive
            );
            if (product == null)
            {
                return NotFound("Product not found or inactive.");
            }

            try
            {
                // Lấy cart của user, bao gồm CartItems
                var cart = (await _shoppingCartService.ListAsync(
                    filter: c => c.UserID == request.UserID,
                    orderBy: null,
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

                // Kiểm tra sản phẩm đã có trong giỏ chưa
                var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == request.ProductId);
                int newQuantity = request.Quantity;
                if (cartItem != null)
                {
                    newQuantity = cartItem.Quantity + request.Quantity;
                }

                // Kiểm tra tổng số lượng không vượt quá tồn kho
                if (newQuantity > product.StockQuantity)
                {
                    return BadRequest($"Cannot add to cart. Total quantity ({newQuantity}) exceeds product stock ({product.StockQuantity}).");
                }

                if (cartItem != null)
                {
                    cartItem.Quantity = newQuantity;
                    await _cartItemService.UpdateAsync(cartItem);
                    await _shoppingCartService.SaveChangesAsync();
                }
                else
                {
                    cartItem = new CartItems
                    {
                        CartItemId = Guid.NewGuid(),
                        CartId = cart.ID,
                        ProductId = request.ProductId,
                        Quantity = request.Quantity,
                    };
                    await _cartItemService.AddAsync(cartItem);
                    await _shoppingCartService.SaveChangesAsync();
                }

                // Sau khi add/update cartItem
                var totalItems = (await _cartItemService.ListAsync(filter: ci => ci.CartId == cart.ID)).Sum(ci => ci.Quantity);

                await _shoppingCartService.UpdateAsync(cart);
                await _shoppingCartService.SaveChangesAsync();

                return Ok(new
                {
                    Message = "Product added to cart successfully.",
                    CartId = cart.ID,
                    TotalItems = totalItems
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpDelete("RemoveFromCart")]
        public async Task<IActionResult> RemoveFromCart([FromBody] RemoveFromCartViewModel request)
        {
            if (request == null)
            {
                return BadRequest("Request data is required.");
            }

            if (string.IsNullOrWhiteSpace(request.UserID))
            {
                return BadRequest("UserID is required.");
            }

            try
            {
                // Lấy cart của user, bao gồm CartItems
                var cart = (await _shoppingCartService.ListAsync(
                    filter: c => c.UserID == request.UserID,
                    orderBy: null,
                    includeProperties: q => q.Include(c => c.CartItems)
                )).FirstOrDefault();

                if (cart == null)
                {
                    return NotFound("Cart not found for this user.");
                }

                // Tìm sản phẩm trong cart
                var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == request.ProductId);
                if (cartItem == null)
                {
                    return NotFound("Product not found in cart.");
                }

                // Xóa cartItem
                await _cartItemService.DeleteAsync(cartItem);
                cart.CartItems.Remove(cartItem); // 🔑 bắt buộc

                // Cập nhật lại tổng số lượng
                var totalItems = (await _cartItemService.ListAsync(
                    filter: ci => ci.CartId == cart.ID
                )).Sum(ci => ci.Quantity);


                await _shoppingCartService.UpdateAsync(cart);
                await _shoppingCartService.SaveChangesAsync();

                return Ok(new
                {
                    Message = "Product removed from cart successfully.",
                    CartId = cart.ID,
                    TotalItems = totalItems
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("GetCartItemsByUser")]
        public async Task<IActionResult> GetCartItemsByUser([FromQuery] string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest("UserID is required.");

            try
            {
                // Kiểm tra userId có tồn tại không
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return NotFound("User does not exist.");

                // Lấy cart của user, bao gồm CartItems và Product
                var cart = (await _shoppingCartService.ListAsync(
                    filter: c => c.UserID == userId,
                    orderBy: null,
                    includeProperties: q => q
                        .Include(c => c.CartItems)
                            .ThenInclude(ci => ci.Product)
                )).FirstOrDefault();

                if (cart == null)
                    return NotFound("Cart not found for this user.");

                if (cart.CartItems == null || !cart.CartItems.Any())
                    return NotFound("Cart does not contain any items.");

                // Lọc chỉ sản phẩm còn hoạt động (không bị xóa)
                var result = cart.CartItems
                    .Where(ci => ci.Product != null && ci.Product.IsActive)
                    .Select(ci => new
                    {
                        ci.ProductId,
                        ProductName = ci.Product.Name,
                        ProductDescription = ci.Product.Description,
                        ProductPrice = ci.Product.Price,
                        ci.Quantity
                    })
                    .ToList();

                if (!result.Any())
                    return NotFound("All products in cart are deleted or inactive.");

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("UpdateCartItemQuantity")]
        public async Task<IActionResult> UpdateCartItemQuantity([FromBody] UpdateCartItemQuantityViewModel request)
        {
            if (request == null)
                return BadRequest("Request data is required.");

            if (string.IsNullOrWhiteSpace(request.UserID))
                return BadRequest("UserID is required.");

            if (request.Quantity < 0)
                return BadRequest("Quantity must be >= 0.");

            try
            {
                // Kiểm tra user tồn tại
                var user = await _userManager.FindByIdAsync(request.UserID);
                if (user == null)
                    return NotFound("User does not exist.");

                // Kiểm tra sản phẩm còn active
                var product = await _productService.FindAsync(p => p.ProductId == request.ProductId && p.IsActive);
                if (product == null)
                    return NotFound("Product not found or inactive.");

                // Lấy cart của user, bao gồm CartItems
                var cart = (await _shoppingCartService.ListAsync(
                    filter: c => c.UserID == request.UserID,
                    orderBy: null,
                    includeProperties: q => q.Include(c => c.CartItems)
                )).FirstOrDefault();

                if (cart == null)
                    return NotFound("Cart not found for this user.");

                var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == request.ProductId);
                if (cartItem == null)
                    return NotFound("Product not found in cart.");

                if (request.Quantity > product.StockQuantity)
                    return BadRequest($"Cannot set quantity greater than product stock ({product.StockQuantity}).");

                if (request.Quantity == 0)
                {
                    // Nếu quantity = 0 thì xóa luôn cart item
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

                return Ok(new
                {
                    Message = "Cart item updated successfully.",
                    CartId = cart.ID,
                    ProductId = cartItem.ProductId,
                    Quantity = request.Quantity
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("Checkout")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutViewModel request)
        {
            if (request == null)
                return BadRequest("Request data is required.");

            if (string.IsNullOrWhiteSpace(request.UserId))
                return BadRequest("UserID is required.");

            if (string.IsNullOrWhiteSpace(request.ShippingAddress))
                return BadRequest("Shipping address is required.");

            if (string.IsNullOrWhiteSpace(request.PaymentMethod))
                return BadRequest("Payment method is required.");

            if (request.ProductIds == null || !request.ProductIds.Any())
                return BadRequest("No products selected for checkout.");

            try
            {
                // Lấy user
                var user = await _userManager.FindByIdAsync(request.UserId);
                if (user == null)
                    return NotFound("User does not exist.");

                // Lấy cart của user, bao gồm CartItems và Product
                var cart = (await _shoppingCartService.ListAsync(
                    filter: c => c.UserID == request.UserId,
                    orderBy: null,
                    includeProperties: q => q
                        .Include(c => c.CartItems)
                            .ThenInclude(ci => ci.Product)
                )).FirstOrDefault();

                if (cart == null || cart.CartItems == null || !cart.CartItems.Any())
                    return BadRequest("Cart is empty.");

                // Lọc các cart item được chọn để checkout
                var selectedCartItems = cart.CartItems
                    .Where(ci => request.ProductIds.Contains(ci.ProductId))
                    .ToList();

                if (!selectedCartItems.Any())
                    return BadRequest("Selected products are not in the cart.");

                // Kiểm tra tồn kho từng sản phẩm được chọn
                foreach (var item in selectedCartItems)
                {
                    if (item.Quantity > item.Product.StockQuantity)
                    {
                        return BadRequest($"Product '{item.Product.Name}' does not have enough stock. Available: {item.Product.StockQuantity}, Requested: {item.Quantity}");
                    }
                }

                // Tạo đơn hàng mới
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
                    // Trừ tồn kho sản phẩm
                    item.Product.StockQuantity -= item.Quantity;
                    await _productService.UpdateAsync(item.Product);

                    // Thêm vào OrderItems
                    order.OrderItems.Add(new OrderItems
                    {
                        OrderItemId = Guid.NewGuid(),
                        OrderId = order.OrderId,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.Product.Price
                    });
                }

                // Lưu đơn hàng
                var orderService = HttpContext.RequestServices.GetService(typeof(BusinessLogic.Services.Order.IOrderService)) as BusinessLogic.Services.Order.IOrderService;
                if (orderService == null)
                    return StatusCode(500, "Order service not available.");

                await orderService.AddAsync(order);
                await orderService.SaveChangesAsync();

                // Xóa các cart item đã checkout khỏi cart
                foreach (var item in selectedCartItems)
                {
                    await _cartItemService.DeleteAsync(item);
                }
                await _shoppingCartService.UpdateAsync(cart);
                await _shoppingCartService.SaveChangesAsync();

                // Lưu thay đổi tồn kho
                await _productService.SaveChangesAsync();

                return Ok(new
                {
                    Message = "Checkout successful.",
                    OrderId = order.OrderId,
                    OrderDate = order.OrderDate
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
