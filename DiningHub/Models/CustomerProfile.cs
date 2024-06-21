using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DiningHub.Areas.Identity.Data;

namespace DiningHub.Models
{
    public class CustomerProfile
    {
        public int CustomerProfileId { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string UserId { get; set; }
        public DiningHubUser User { get; set; }
    }
}
