using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class ProductImages
    {
        [Key]
        public Guid ProductImageID { get; set; } = Guid.NewGuid();
        public string ImageUrl { get; set; }

        [ForeignKey("Product")]
        public Guid ProductID { get; set; }
        public virtual Products Product { get; set; }
    }
}
