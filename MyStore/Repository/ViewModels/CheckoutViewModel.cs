namespace Repository.ViewModels
{
    public class CheckoutViewModel
    {

        public string UserId { get; set; }
        public string ShippingAddress { get; set; }
        public string PaymentMethod { get; set; } // e.g., "COD"
        public List<Guid> ProductIds { get; set; } // Danh sách sản phẩm muốn checkout

    }
}
