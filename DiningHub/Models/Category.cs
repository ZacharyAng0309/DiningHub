using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DiningHub.Models
{
    public class Category
    {
        public int CategoryId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
        public ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
    }
}
