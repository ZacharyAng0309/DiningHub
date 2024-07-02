using Microsoft.AspNetCore.Mvc;
using DiningHub.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DiningHub.Areas.Identity.Data;

namespace DiningHub.Controllers
{
    public class MenuController : Controller
    {
        private readonly DiningHubContext _context;

        public MenuController(DiningHubContext context)
        {
            _context = context;
        }

        // View all menu items
        public async Task<IActionResult> Index()
        {
            var menuItems = await _context.MenuItems.ToListAsync();
            return View(menuItems);
        }

        // View details of a single menu item
        public async Task<IActionResult> Details(int id)
        {
            var menuItem = await _context.MenuItems.FirstOrDefaultAsync(m => m.MenuItemId == id);
            if (menuItem == null)
            {
                return NotFound();
            }
            return View(menuItem);
        }
    }
}
