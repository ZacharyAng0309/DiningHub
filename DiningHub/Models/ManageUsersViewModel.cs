using DiningHub.Areas.Identity.Data;
using X.PagedList;

namespace DiningHub.Models
{
    public class ManageUsersViewModel
    {
        public IPagedList<DiningHubUser> Users { get; set; }
        public Dictionary<string, IList<string>> UserRoles { get; set; }

    }
}
