using Microsoft.AspNetCore.Http;

namespace Repository.ViewModels
{
    public class CreateProductViewModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public Guid CategoryID { get; set; }
        //public string ImageBase64 { get; set; } // Chuỗi base64 của ảnh
        public List<IFormFile> Images { get; set; }
    }
}
