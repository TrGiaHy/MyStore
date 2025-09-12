namespace Repository.ViewModels
{
    public class UpdateCartItemQuantityViewModel
    {
        public string UserID { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
