using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class ShoppingCarts
    {
        [Key]
        public Guid ID { get; set; } = Guid.NewGuid();
        public int Quantity { get; set; }

        [ForeignKey("AppUser")]
        public string UserID { get; set; }
        public virtual AppUser AppUser { get; set; }
        [ForeignKey("Product")]
        
        // Navigation
        public ICollection<CartItems> CartItems { get; set; }
    }
}
