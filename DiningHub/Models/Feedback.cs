using DiningHub.Areas.Identity.Data;
using System;
using System.ComponentModel.DataAnnotations;

namespace DiningHub.Models
{
    public class Feedback
    {
        public int FeedbackId { get; set; }

        public string UserId { get; set; }
        public DiningHubUser User { get; set; }

        [Required]
        [StringLength(1000)]
        public string Comments { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }  // e.g., rating out of 5

        [Required]
        public DateTime Date { get; set; }
    }
}
