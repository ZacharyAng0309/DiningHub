namespace DiningHub.Models
{
    public class CustomerOrderDetailsViewModel
    {
        public Order Order { get; set; }
        public Feedback Feedback { get; set; }
        public bool CanProvideFeedback { get; set; }
    }
}
