using System;
using System.ComponentModel.DataAnnotations;

namespace DiningHub.Models
{
    public class Receipt
    {
        public int ReceiptId { get; set; }

        [Required]
        public int OrderId { get; set; }
        public Order Order { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal TotalAmount { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime DateIssued { get; set; }
    }
}
