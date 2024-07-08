using System.ComponentModel.DataAnnotations;
using DiningHub.Helper;

namespace DiningHub.Models
{
    public class ShoppingCartItem
    {
        public int ShoppingCartItemId { get; set; }

        [Required]
        public int MenuItemId { get; set; }
        public MenuItem MenuItem { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        public string UserId { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTimeHelper.GetMalaysiaTime();
    }
}
