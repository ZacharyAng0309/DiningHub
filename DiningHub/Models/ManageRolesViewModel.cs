using Microsoft.AspNetCore.Identity;

namespace DiningHub.Models
{
    public class ManageRolesViewModel
    {
        public string UserId { get; set; }
        public IList<IdentityRole> AvailableRoles { get; set; }
        public IList<string> UserRoles { get; set; }
        public string[] SelectedRoles { get; set; }
    }
}

