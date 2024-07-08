using System;

namespace DiningHub.Models
{
    public class CustomerOrderViewModel
    {
        public Order Order { get; set; }
        public bool HasFeedback { get; set; }
        public bool CanProvideFeedback { get; set; }
        public int? FeedbackId { get; set; }
    }
}
