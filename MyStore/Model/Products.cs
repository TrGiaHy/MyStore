using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class Products
    {
        [Key]
        public Guid ProductId { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [ForeignKey("Category")]
        public Guid CategoryID { get; set; }
        public virtual Categories Category { get; set; }
        public ICollection<CartItems> CartItems { get; set; }
        public ICollection<ProductImages> ProductImages { get; set; }
        public ICollection<OrderItems> OrderItems { get; set; }
    }
}
