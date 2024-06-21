using System.ComponentModel.DataAnnotations;

namespace DiningHub.Models
{
    public class Inventory
    {
        public int InventoryId { get; set; }
        public string ItemName { get; set; }
        public int Quantity { get; set; }
        public int MenuId { get; set; }
        public Menu Menu { get; set; }
    }
}
