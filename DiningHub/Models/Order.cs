using DiningHub.Areas.Identity.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DiningHub.Models
{
    public class Order
    {
        public int OrderId { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal TotalAmount { get; set; }

        public string UserId { get; set; }
        public DiningHubUser User { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }

        public Feedback Feedback { get; set; }
    }
}
