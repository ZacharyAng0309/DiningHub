using System.ComponentModel.DataAnnotations;

namespace DiningHub.Models
{
    public class InventoryItem
    {
        public int InventoryItemId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        public DateTime LastUpdated { get; set; }
    }
}
