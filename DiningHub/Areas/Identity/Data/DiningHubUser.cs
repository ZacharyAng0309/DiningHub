using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace DiningHub.Areas.Identity.Data;

// Add profile data for application users by adding properties to the DiningHubUser class
public class DiningHubUser : IdentityUser
{
    public string Address { get; set; }
}

