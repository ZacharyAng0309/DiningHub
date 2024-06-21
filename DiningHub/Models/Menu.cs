using System.ComponentModel.DataAnnotations;

namespace DiningHub.Models
{
    public class Menu
    {
        public int MenuId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }
        public ICollection<Inventory> Inventories { get; set; }
    }
}
