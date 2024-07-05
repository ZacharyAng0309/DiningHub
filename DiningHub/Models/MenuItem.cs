using DiningHub.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;

namespace DiningHub.Models
{
    public class MenuItem
    {
        public int MenuItemId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        [StringLength(50)]
        public string Category { get; set; }

        public string ImageUrl { get; set; }

        public bool IsAvailable { get; set; }

        public string CreatedById { get; set; }
        public DiningHubUser CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
