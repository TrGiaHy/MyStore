using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class CartItems
    {
        [Key]
        public Guid CartItemId { get; set; } = Guid.NewGuid();
        public int Quantity { get; set; }

        // Foreign keys
        public Guid CartId { get; set; }
        public ShoppingCarts Cart { get; set; }

        public Guid ProductId { get; set; }
        public Products Product { get; set; }
    }
}
