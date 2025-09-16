namespace Repository.ViewModels
{
    public class CartItemViewModel
    {
        public Guid CartItemId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public string ProductImage { get; set; }
        public decimal ProductPrice { get; set; }
        public int Quantity { get; set; }
    }
}
