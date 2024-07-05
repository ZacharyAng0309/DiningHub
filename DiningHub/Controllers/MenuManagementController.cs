using Microsoft.AspNetCore.Mvc;
using DiningHub.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using DiningHub.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using System.Linq;

namespace DiningHub.Controllers
{
    [Authorize(Policy = "RequireInternalRole")]
    public class MenuManagementController : Controller
    {
        private readonly DiningHubContext _context;
        private readonly UserManager<DiningHubUser> _userManager;

        public MenuManagementController(DiningHubContext context, UserManager<DiningHubUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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

        // Create a new menu item (GET)
        public IActionResult Create()
        {
            return View();
        }

        // Create a new menu item (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MenuItem menuItem)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                menuItem.CreatedById = user.Id;
                menuItem.CreatedAt = DateTime.UtcNow;
                menuItem.UpdatedAt = DateTime.UtcNow;

                _context.Add(menuItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(menuItem);
        }

        // Edit a menu item (GET)
        public async Task<IActionResult> Edit(int id)
        {
            var menuItem = await _context.MenuItems.FindAsync(id);
            if (menuItem == null)
            {
                return NotFound();
            }
            return View(menuItem);
        }

        // Edit a menu item (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MenuItem menuItem)
        {
            if (id != menuItem.MenuItemId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    var existingMenuItem = await _context.MenuItems.FindAsync(id);

                    if (existingMenuItem == null)
                    {
                        return NotFound();
                    }

                    existingMenuItem.Name = menuItem.Name;
                    existingMenuItem.Description = menuItem.Description;
                    existingMenuItem.Price = menuItem.Price;
                    existingMenuItem.Category = menuItem.Category;
                    existingMenuItem.ImageUrl = menuItem.ImageUrl;
                    existingMenuItem.IsAvailable = menuItem.IsAvailable;
                    existingMenuItem.UpdatedAt = DateTime.UtcNow;
                    existingMenuItem.CreatedById = user.Id;

                    _context.Update(existingMenuItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MenuItemExists(menuItem.MenuItemId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(menuItem);
        }

        // Delete a menu item (GET)
        public async Task<IActionResult> Delete(int id)
        {
            var menuItem = await _context.MenuItems.FirstOrDefaultAsync(m => m.MenuItemId == id);
            if (menuItem == null)
            {
                return NotFound();
            }
            return View(menuItem);
        }

        // Delete a menu item (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var menuItem = await _context.MenuItems.FindAsync(id);
            _context.MenuItems.Remove(menuItem);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MenuItemExists(int id)
        {
            return _context.MenuItems.Any(e => e.MenuItemId == id);
        }
    }
}
