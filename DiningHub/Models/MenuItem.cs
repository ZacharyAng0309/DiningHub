using DiningHub.Areas.Identity.Data;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiningHub.Models
{
    public class MenuItem
    {
        public int MenuItemId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        [Range(0.01, 10000.00)]
        public decimal Price { get; set; }

        [Required]
        [ForeignKey("Category")]
        public int CategoryId { get; set; }

        public string ImageUrl { get; set; }

        public bool IsAvailable { get; set; }

        [Required]
        public string CreatedById { get; set; }

        [Required]
        public string LastUpdatedById { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public virtual Category Category { get; set; }
        public virtual DiningHubUser CreatedBy { get; set; }
        public virtual DiningHubUser LastUpdatedBy { get; set; }
    }
}
