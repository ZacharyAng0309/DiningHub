using DiningHub.Areas.Identity.Data;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiningHub.Models
{
    public class InventoryItem
    {
        public int InventoryItemId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }

        [Required]
        [ForeignKey("Category")]
        public int CategoryId { get; set; }

        public string CreatedById { get; set; }
        public string LastUpdatedById { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public virtual Category Category { get; set; }
        public virtual DiningHubUser CreatedBy { get; set; }
        public virtual DiningHubUser LastUpdatedBy { get; set; }
    }
}
