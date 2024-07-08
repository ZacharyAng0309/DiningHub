using DiningHub.Helpers;
using System.Collections.Generic;

namespace DiningHub.Models
{
    public class MenuViewModel
    {
        public PaginatedList<MenuItem> MenuItems { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string SearchString { get; set; }
        public int? CategoryId { get; set; }
    }
}
