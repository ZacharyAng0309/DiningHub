using DiningHub.Areas.Identity.Data;
using DiningHub.Helpers;

namespace DiningHub.Models
{
    public class ManageUsersViewModel
    {
        public PaginatedList<DiningHubUser> Users { get; set; }
        public Dictionary<string, IList<string>> UserRoles { get; set; }
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }

}
