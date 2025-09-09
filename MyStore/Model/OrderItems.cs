using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class OrderItems
    {
        public Guid OrderItemId { get; set; } = Guid.NewGuid();
        public int Quantity { get; set; }
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
