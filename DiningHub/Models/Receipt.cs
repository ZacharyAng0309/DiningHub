using System;
using System.ComponentModel.DataAnnotations;

namespace DiningHub.Models
{
    public class Receipt
    {
        public int ReceiptId { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; }

        public DateTime ReceiptDate { get; set; }

        [Required]
        [Range(0, 10000)]
        public decimal TotalAmount { get; set; }
    }
}
