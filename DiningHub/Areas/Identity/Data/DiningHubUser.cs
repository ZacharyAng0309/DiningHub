using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using DiningHub.Helper;
using DiningHub.Models;

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
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        public bool IsDeleted { get; set; }

        [RegularExpression(@"\d{10,}", ErrorMessage = "The phone number must contain at least 10 digits.")]
        public override string PhoneNumber { get; set; }

        [BindNever]
        public ICollection<Order> Orders { get; set; }

        [BindNever]
        public ICollection<Feedback> Feedbacks { get; set; }

        public DateTime CreatedAt { get; set; } = DateTimeHelper.GetMalaysiaTime();
        public DateTime UpdatedAt { get; set; } = DateTimeHelper.GetMalaysiaTime();
    }
}
