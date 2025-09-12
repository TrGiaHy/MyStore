using BusinessLogic.Services.Order;
using BusinessLogic.Services.OrderItem;
using BusinessLogic.Services.Product;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MyStore.Web.APIControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IOrderItemService _orderItemService;
        private readonly IProductService _productService;

        public OrdersController(IOrderService orderService, IOrderItemService orderItemService, IProductService productService)
        {
            _orderService = orderService;
            _orderItemService = orderItemService;
            _productService = productService;
        }

        [HttpGet("GetOrderHistoryByUser/{userId}")]
        public async Task<IActionResult> GetOrderHistoryByUser(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest("UserId is required.");

            try
            {
                // Lấy danh sách order của user, bao gồm OrderItems
                var orders = await _orderService.ListAsync(
                    filter: o => o.UserId == userId,
                    orderBy: q => q.OrderByDescending(o => o.OrderDate),
                    includeProperties: q => q.Include(o => o.OrderItems)
                );

                if (orders == null || !orders.Any())
                    return Ok(new { Orders = new List<object>() });

                // Tính tổng tiền và tổng số lượng sản phẩm cho từng order
                var orderList = orders.Select(o => new
                {
                    o.OrderId,
                    o.OrderDate,
                    o.Status,
                    o.ShippingAddress,
                    o.PaymentMethod,
                    TotalAmount = o.OrderItems?.Sum(oi => oi.Quantity * oi.UnitPrice) ?? 0,
                    TotalQuantity = o.OrderItems?.Sum(oi => oi.Quantity) ?? 0
                }).ToList();

                return Ok(new
                {
                    Orders = orderList
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("GetOrderProducts/{orderId:guid}")]
        public async Task<IActionResult> GetOrderProducts(Guid orderId)
        {
            try
            {
                // Lấy order kèm OrderItems và Product
                var orders = await _orderService.ListAsync(
                    filter: o => o.OrderId == orderId,
                    includeProperties: q => q.Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                );

                var order = orders.FirstOrDefault();
                if (order == null)
                    return NotFound("Order not found.");

                var products = order.OrderItems?.Select(oi => new
                {
                    oi.ProductId,
                    ProductName = oi.Product?.Name,
                    ProductDescription = oi.Product?.Description,
                    ProductPrice = oi.Product?.Price,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.Quantity * oi.UnitPrice
                }).ToList();

                return Ok(new
                {
                    OrderId = order.OrderId,
                    Products = products
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPatch("CancelOrder/{orderId:guid}")]
        public async Task<IActionResult> CancelOrder(Guid orderId)
        {
            try
            {
                // Lấy order theo id
                var order = await _orderService.ListAsync(
                    filter: o => o.OrderId == orderId,
                    includeProperties: q => q.Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                );
                var orderEntity = order.FirstOrDefault();
                if (orderEntity == null)
                    return NotFound("Order not found.");

                // Chỉ cho phép hủy nếu trạng thái là Pending
                if (!string.Equals(orderEntity.Status, "Pending", StringComparison.OrdinalIgnoreCase))
                    return BadRequest("Order cannot be canceled. Only orders with status 'Pending' can be canceled.");

                // Cộng lại số lượng stock cho từng sản phẩm trong order
                if (orderEntity.OrderItems != null)
                {
                    foreach (var item in orderEntity.OrderItems)
                    {
                        if (item.Product != null)
                        {
                            item.Product.StockQuantity += item.Quantity;
                            await _productService.UpdateAsync(item.Product);
                        }
                    }
                    await _productService.SaveChangesAsync();
                }

                orderEntity.Status = "Cancel by Customer";
                await _orderService.UpdateAsync(orderEntity);
                await _orderService.SaveChangesAsync();

                return Ok(new { Message = "Order canceled successfully.", OrderId = orderEntity.OrderId, Status = orderEntity.Status });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPatch("UpdateOrderStatus/{orderId:guid}")]
        public async Task<IActionResult> UpdateOrderStatus(Guid orderId, [FromBody] string newStatus)
        {
            // Danh sách trạng thái hợp lệ
            var validStatuses = new[] { "Shipping", "Completed", "Canceled by Seller" };

            if (string.IsNullOrWhiteSpace(newStatus) || !validStatuses.Contains(newStatus))
                return BadRequest($"Invalid status. Allowed values: {string.Join(", ", validStatuses)}");

            try
            {
                var order = await _orderService.FindAsync(o => o.OrderId == orderId);
                if (order == null)
                    return NotFound("Order not found.");

                // Không cho cập nhật nếu đã là Completed hoặc Canceled
                if (string.Equals(order.Status, "Completed", StringComparison.OrdinalIgnoreCase) ||
                    order.Status?.StartsWith("Cancel", StringComparison.OrdinalIgnoreCase) == true)
                {
                    return BadRequest("Order cannot be updated. It is already completed or canceled.");
                }

                order.Status = newStatus;
                await _orderService.UpdateAsync(order);
                await _orderService.SaveChangesAsync();

                return Ok(new { Message = "Order status updated successfully.", OrderId = order.OrderId, Status = order.Status });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
