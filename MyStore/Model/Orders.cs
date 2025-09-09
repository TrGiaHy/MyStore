using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class Orders
    {
        [Key]
        public Guid OrderId { get; set; } = Guid.NewGuid();
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public string ShippingAddress { get; set; }
        public string PaymentMethod { get; set; } // e.g., "COD"
        public string Status { get; set; } // Pending, Shipping, Completed, Canceled

        // Foreign key
        [ForeignKey("AppUser")]
        public string UserId { get; set; }
        public virtual AppUser User { get; set; }

        // Navigation
        public ICollection<OrderItems> OrderItems { get; set; }
    }
}
