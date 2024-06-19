using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DiningHub.Areas.Identity.Data;

namespace DiningHub.Models
{
    public class CustomerProfile
    {
        [Key]
        public int CustomerProfileId { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }
        public DiningHubUser User { get; set; }

        public int Points { get; set; }
        // Add other customer-specific properties here
    }
}
