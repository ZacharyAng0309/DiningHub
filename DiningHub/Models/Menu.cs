using System.ComponentModel.DataAnnotations;

namespace DiningHub.Models
{
    public class Menu
    {
        public int MenuId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        [Range(0, 10000)]
        public decimal Price { get; set; }
    }
}
