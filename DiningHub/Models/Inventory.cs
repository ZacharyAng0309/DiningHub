using System.ComponentModel.DataAnnotations;

namespace DiningHub.Models
{
    public class Inventory
    {
        public int InventoryId { get; set; }

        [Required]
        [StringLength(100)]
        public string ItemName { get; set; }

        [Required]
        [Range(0, 10000)]
        public int Quantity { get; set; }

        [Required]
        [Range(0, 10000)]
        public decimal Price { get; set; }
    }
}
