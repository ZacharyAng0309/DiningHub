using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using DiningHub.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DiningHub.Areas.Identity.Data
{
    public class DiningHubUser : IdentityUser
    {
        public DiningHubUser()
        {
            Orders = new HashSet<Order>();
            Feedbacks = new HashSet<Feedback>();
        }

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

        [BindNever]
        public ICollection<Order> Orders { get; set; }

        [BindNever]
        public ICollection<Feedback> Feedbacks { get; set; }
    }
}
