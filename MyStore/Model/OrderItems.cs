using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Model
{
    public class OrderItems
    {
        [Key]
        public Guid OrderItemId { get; set; } = Guid.NewGuid();
        public int Quantity { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        // Foreign keys
        [ForeignKey("Order")]
        public Guid OrderId { get; set; }
        public virtual Orders Order { get; set; }
        [ForeignKey("Product")]
        public Guid ProductId { get; set; }
        public virtual Products Product { get; set; }
    }
}
