﻿using DiningHub.Areas.Identity.Data;
using System;
using System.ComponentModel.DataAnnotations;

namespace DiningHub.Models
{
    public class Feedback
    {
        public int FeedbackId { get; set; }

        [Required]
        public string UserId { get; set; }
        public virtual DiningHubUser User { get; set; }

        [Required]
        [StringLength(1000)]
        public string Comments { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; }

        [Required]
        public DateTime Date { get; set; } 

        [Required]
        public int OrderId { get; set; }
        public virtual Order Order { get; set; }
    }
}
