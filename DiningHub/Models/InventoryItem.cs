using System;
using System.ComponentModel.DataAnnotations;
using DiningHub.Areas.Identity.Data;

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

        public string CreatedById { get; set; }
        public DiningHubUser CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
