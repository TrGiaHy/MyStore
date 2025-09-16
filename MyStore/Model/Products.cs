using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model
{
    public class Products
    {
        [Key]
        public Guid ProductId { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Description { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true; //true = show, false = hide

        [ForeignKey("Category")]
        public Guid CategoryID { get; set; }
        public virtual Categories Category { get; set; }
        public ICollection<CartItems> CartItems { get; set; }
        public ICollection<ProductImages> ProductImages { get; set; }
        public ICollection<OrderItems> OrderItems { get; set; }
    }
}
