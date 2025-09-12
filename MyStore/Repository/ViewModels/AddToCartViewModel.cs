namespace Repository.ViewModels
{
    public class AddToCartViewModel
    {
        public string UserID { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
