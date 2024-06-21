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
        public string Comments { get; set; }
        public DateTime Date { get; set; }
    }
}
