using Microsoft.AspNetCore.Mvc;
using DiningHub.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DiningHub.Areas.Identity.Data;
using System.Linq;
using DiningHub.Helpers;

namespace DiningHub.Controllers
{
    public class MenuController : Controller
    {
        private readonly DiningHubContext _context;

        public MenuController(DiningHubContext context)
        {
            _context = context;
        }

        // View all menu items with search, sort, filter, and pagination
        public async Task<IActionResult> Index(string searchString, int? categoryId, string sortOrder, int? page)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["PriceSortParm"] = sortOrder == "Price" ? "price_desc" : "Price";
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentCategory"] = categoryId;

            ViewBag.Categories = await _context.Categories.ToListAsync();

            var menuItems = from m in _context.MenuItems.Include(m => m.Category)
                            where m.IsAvailable && !m.IsDeleted
                            select m;

            if (!string.IsNullOrEmpty(searchString))
            {
                menuItems = menuItems.Where(m => m.Name.Contains(searchString) || m.Description.Contains(searchString));
            }

            if (categoryId.HasValue)
            {
                menuItems = menuItems.Where(m => m.CategoryId == categoryId.Value);
            }

            switch (sortOrder)
            {
                case "name_desc":
                    menuItems = menuItems.OrderByDescending(m => m.Name);
                    break;
                case "Price":
                    menuItems = menuItems.OrderBy(m => m.Price);
                    break;
                case "price_desc":
                    menuItems = menuItems.OrderByDescending(m => m.Price);
                    break;
                default:
                    menuItems = menuItems.OrderBy(m => m.Name);
                    break;
            }

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            var pagedMenuItems = await PaginatedList<MenuItem>.CreateAsync(menuItems.AsNoTracking(), pageNumber, pageSize);

            var viewModel = new MenuViewModel
            {
                MenuItems = pagedMenuItems,
                CurrentPage = pageNumber,
                TotalPages = pagedMenuItems.TotalPages,
                SearchString = searchString,
                CategoryId = categoryId
            };

            return View(viewModel);
        }

        // View details of a single menu item
        public async Task<IActionResult> Details(int id)
        {
            var menuItem = await _context.MenuItems.Include(m => m.Category).FirstOrDefaultAsync(m => m.MenuItemId == id);
            if (menuItem == null)
            {
                return NotFound();
            }
            return View(menuItem);
        }
    }
}
