using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using DiningHub.Models;
using Microsoft.AspNetCore.Identity;

namespace DiningHub.Areas.Identity.Data
{
    public class DiningHubUser : IdentityUser
    {
        [PersonalData]
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [PersonalData]
        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [PersonalData]
        public int Points { get; set; }

        public ICollection<Order> Orders { get; set; }
        public ICollection<Feedback> Feedbacks { get; set; }
    }
}
