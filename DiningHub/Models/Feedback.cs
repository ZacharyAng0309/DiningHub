using System;
using System.ComponentModel.DataAnnotations;

namespace DiningHub.Models
{
    public class Feedback
    {
        public int FeedbackId { get; set; }

        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        [Required]
        [StringLength(1000)]
        public string Comment { get; set; }

        public DateTime Date { get; set; }
    }
}
