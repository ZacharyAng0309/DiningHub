using DiningHub.Helpers;
using System.Collections.Generic;

namespace DiningHub.Models
{
    public class MenuManagementViewModel
    {
        public PaginatedList<MenuItem> MenuItems { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string CurrentSort { get; set; }
        public string CurrentFilter { get; set; }
        public int? CurrentCategory { get; set; }
    }

}
