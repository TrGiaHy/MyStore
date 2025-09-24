namespace Repository.ViewModels
{
    public class OrderSummaryViewModel
    {
        public Guid OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public string ShippingAddress { get; set; }
        public string PaymentMethod { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalQuantity { get; set; }
    }
    public class OrderHistoryResponse
    {
        public List<OrderSummaryViewModel> Orders { get; set; } = new();
    }
    public class OrderProductItemViewModel
    {
        public Guid ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? ProductDescription { get; set; }
        public decimal? ProductPrice { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class OrderProductsResponse
    {
        public Guid OrderId { get; set; }
        public List<OrderProductItemViewModel> Products { get; set; } = new();
    }
}
