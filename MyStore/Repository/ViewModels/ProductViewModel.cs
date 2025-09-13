namespace Repository.ViewModels
{
    public class ProductViewModel
    {
        public Guid ProductId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }
        public Guid CategoryID { get; set; }
        public string CategoryName { get; set; }
        public List<string> Images { get; set; }
    }
}
