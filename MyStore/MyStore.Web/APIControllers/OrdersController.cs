using BusinessLogic.Services.Order;
using BusinessLogic.Services.OrderItem;
using BusinessLogic.Services.Product;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static BusinessLogic.Services.ApiClientService.ApiClientService;

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
            {
                return Ok(new ApiResponse<string>
                {
                    Success = false,
                    ErrorMessage = "UserId is required.",
                    StatusCode = 400
                });
            }

            try
            {
                var orders = await _orderService.ListAsync(
                    filter: o => o.UserId == userId,
                    orderBy: q => q.OrderByDescending(o => o.OrderDate),
                    includeProperties: q => q.Include(o => o.OrderItems)
                );

                if (orders == null || !orders.Any())
                {
                    return Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Data = new { Orders = new List<object>() },
                        StatusCode = 200
                    });
                }

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

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = new { Orders = orderList },
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

        [HttpGet("GetOrderProducts/{orderId:guid}")]
        public async Task<IActionResult> GetOrderProducts(Guid orderId)
        {
            try
            {
                var orders = await _orderService.ListAsync(
                    filter: o => o.OrderId == orderId,
                    includeProperties: q => q.Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                );

                var order = orders.FirstOrDefault();
                if (order == null)
                {
                    return Ok(new ApiResponse<string>
                    {
                        Success = false,
                        ErrorMessage = "Order not found.",
                        StatusCode = 404
                    });
                }

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

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = new { OrderId = order.OrderId, Products = products },
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

        [HttpPatch("CancelOrder/{orderId:guid}")]
        public async Task<IActionResult> CancelOrder(Guid orderId)
        {
            try
            {
                var order = await _orderService.ListAsync(
                    filter: o => o.OrderId == orderId,
                    includeProperties: q => q.Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                );
                var orderEntity = order.FirstOrDefault();

                if (orderEntity == null)
                {
                    return Ok(new ApiResponse<string>
                    {
                        Success = false,
                        ErrorMessage = "Order not found.",
                        StatusCode = 404
                    });
                }

                if (!string.Equals(orderEntity.Status, "Pending", StringComparison.OrdinalIgnoreCase))
                {
                    return Ok(new ApiResponse<string>
                    {
                        Success = false,
                        ErrorMessage = "Order cannot be canceled. Only orders with status 'Pending' can be canceled.",
                        StatusCode = 400
                    });
                }

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

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = new { Message = "Order canceled successfully.", OrderId = orderEntity.OrderId, Status = orderEntity.Status },
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

        [HttpPatch("UpdateOrderStatus/{orderId:guid}")]
        public async Task<IActionResult> UpdateOrderStatus(Guid orderId, [FromBody] string newStatus)
        {
            var validStatuses = new[] { "Shipping", "Completed", "Canceled by Seller" };

            if (string.IsNullOrWhiteSpace(newStatus) || !validStatuses.Contains(newStatus))
            {
                return Ok(new ApiResponse<string>
                {
                    Success = false,
                    ErrorMessage = $"Invalid status. Allowed values: {string.Join(", ", validStatuses)}",
                    StatusCode = 400
                });
            }

            try
            {
                var order = await _orderService.FindAsync(o => o.OrderId == orderId);
                if (order == null)
                {
                    return Ok(new ApiResponse<string>
                    {
                        Success = false,
                        ErrorMessage = "Order not found.",
                        StatusCode = 404
                    });
                }

                if (string.Equals(order.Status, "Completed", StringComparison.OrdinalIgnoreCase) ||
                    order.Status?.StartsWith("Cancel", StringComparison.OrdinalIgnoreCase) == true)
                {
                    return Ok(new ApiResponse<string>
                    {
                        Success = false,
                        ErrorMessage = "Order cannot be updated. It is already completed or canceled.",
                        StatusCode = 400
                    });
                }

                order.Status = newStatus;
                await _orderService.UpdateAsync(order);
                await _orderService.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = new { Message = "Order status updated successfully.", OrderId = order.OrderId, Status = order.Status },
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
