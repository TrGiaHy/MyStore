using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class Categories
    {
        [Key]
        public Guid CategoryId { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; } = true; //true = show, false = hide
        // Navigation
        public ICollection<Products> Products { get; set; }
    }
}
