using DiningHub.Helpers;
using System.Collections.Generic;

namespace DiningHub.Models
{
    public class MenuManagementViewModel
    {
        public PaginatedList<MenuItem> MenuItems { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
