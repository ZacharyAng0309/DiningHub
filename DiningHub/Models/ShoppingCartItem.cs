using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DiningHub.Areas.Identity.Data;
using DiningHub.Helper;

namespace DiningHub.Models
{
    public class ShoppingCartItem
    {
        public int ShoppingCartItemId { get; set; }

        [Required]
        [ForeignKey("MenuItem")]
        public int MenuItemId { get; set; }

        public MenuItem MenuItem { get; set; }

        [Required]
        [ForeignKey("User")]
        public string UserId { get; set; }

        public DiningHubUser User { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTimeHelper.GetMalaysiaTime();
    }
}
