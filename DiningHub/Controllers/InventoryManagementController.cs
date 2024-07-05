using Microsoft.AspNetCore.Mvc;
using DiningHub.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using DiningHub.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;

namespace DiningHub.Controllers
{
    [Authorize(Policy = "RequireInternalRole")]
    [Route("manage/inventory")]
    public class InventoryManagementController : Controller
    {
        private readonly DiningHubContext _context;
        private readonly UserManager<DiningHubUser> _userManager;

        public InventoryManagementController(DiningHubContext context, UserManager<DiningHubUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // View all inventory items
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var inventoryItems = await _context.InventoryItems
                .Include(i => i.CreatedBy)
                .ToListAsync();
            return View(inventoryItems);
        }

        // View details of a single inventory item
        [HttpGet("details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var inventoryItem = await _context.InventoryItems
                .Include(i => i.CreatedBy)
                .FirstOrDefaultAsync(i => i.InventoryItemId == id);
            if (inventoryItem == null)
            {
                return NotFound();
            }
            return View(inventoryItem);
        }

        // Create a new inventory item (GET)
        [HttpGet("create")]
        public IActionResult Create()
        {
            return View();
        }

        // Create a new inventory item (POST)
        [HttpPost("create")]
        public async Task<IActionResult> Create(InventoryItem inventoryItem)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                inventoryItem.CreatedById = user.Id;
                inventoryItem.CreatedAt = DateTime.UtcNow;
                inventoryItem.UpdatedAt = DateTime.UtcNow;

                _context.Add(inventoryItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(inventoryItem);
        }

        // Edit an inventory item (GET)
        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var inventoryItem = await _context.InventoryItems.FindAsync(id);
            if (inventoryItem == null)
            {
                return NotFound();
            }
            return View(inventoryItem);
        }

        // Edit an inventory item (POST)
        [HttpPost("edit/{id}")]
        public async Task<IActionResult> Edit(int id, InventoryItem inventoryItem)
        {
            if (id != inventoryItem.InventoryItemId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    inventoryItem.UpdatedAt = DateTime.UtcNow;
                    _context.Update(inventoryItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InventoryItemExists(inventoryItem.InventoryItemId))
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
            return View(inventoryItem);
        }

        // Delete an inventory item (GET)
        [HttpGet("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var inventoryItem = await _context.InventoryItems
                .Include(i => i.CreatedBy)
                .FirstOrDefaultAsync(i => i.InventoryItemId == id);
            if (inventoryItem == null)
            {
                return NotFound();
            }
            return View(inventoryItem);
        }

        // Delete an inventory item (POST)
        [HttpPost("delete/{id}")]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var inventoryItem = await _context.InventoryItems.FindAsync(id);
            _context.InventoryItems.Remove(inventoryItem);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool InventoryItemExists(int id)
        {
            return _context.InventoryItems.Any(e => e.InventoryItemId == id);
        }
    }
}
