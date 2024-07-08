using DiningHub.Areas.Identity.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DiningHub.Helper;

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
        public string UserName { get; set; }
        public DiningHubUser User { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }

        public Feedback Feedback { get; set; }

        public string PaymentMethod { get; set; }
        public DateTime? PaymentDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTimeHelper.GetMalaysiaTime();
    }
}
