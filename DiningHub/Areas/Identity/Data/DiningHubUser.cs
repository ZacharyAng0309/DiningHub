using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DiningHub.Models;
using Microsoft.AspNetCore.Identity;

namespace DiningHub.Areas.Identity.Data
{
    public class DiningHubUser : IdentityUser
    {
        public int Points { get; set; }
        public CustomerProfile Profile { get; set; }
        public ICollection<Order> Orders { get; set; }
        public ICollection<Feedback> Feedbacks { get; set; }
    }
}
