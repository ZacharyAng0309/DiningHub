using Microsoft.AspNetCore.Mvc;
using DiningHub.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using DiningHub.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<InventoryManagementController> _logger;

        public InventoryManagementController(DiningHubContext context, UserManager<DiningHubUser> userManager, ILogger<InventoryManagementController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // View all inventory items
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var inventoryItems = await _context.InventoryItems
                .Include(i => i.CreatedBy)
                .Include(i => i.LastUpdatedBy)
                .Include(i => i.Category)
                .ToListAsync();
            return View(inventoryItems);
        }

        // View details of a single inventory item
        [HttpGet("details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var inventoryItem = await _context.InventoryItems
                .Include(i => i.CreatedBy)
                .Include(i => i.LastUpdatedBy)
                .Include(i => i.Category)
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
            ViewBag.Categories = _context.Categories.ToList();
            return View();
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InventoryItem inventoryItem)
        {
            // Get the current user
            var user = await _userManager.GetUserAsync(User);

            // Ensure user is found
            if (user == null)
            {
                ModelState.AddModelError("", "User not found.");
                ViewBag.Categories = _context.Categories.ToList();
                return View(inventoryItem);
            }

            // Ensure category is valid
            var category = await _context.Categories.FindAsync(inventoryItem.CategoryId);
            if (category == null)
            {
                ModelState.AddModelError("CategoryId", "Invalid category selected.");
                ViewBag.Categories = _context.Categories.ToList();
                return View(inventoryItem);
            }

            // Populate required fields
            inventoryItem.CreatedById = user.Id;
            inventoryItem.LastUpdatedById = user.Id;
            inventoryItem.CreatedAt = DateTime.UtcNow;
            inventoryItem.UpdatedAt = DateTime.UtcNow;
            inventoryItem.Category = category;
            inventoryItem.CreatedBy = user;
            inventoryItem.LastUpdatedBy = user;

            // Clear existing validation errors to revalidate the model with the new values
            ModelState.Clear();
            TryValidateModel(inventoryItem);

            if (ModelState.IsValid)
            {
                _context.Add(inventoryItem);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Inventory item '{inventoryItem.Name}' created successfully.");
                return RedirectToAction(nameof(Index));
            }

            // Log ModelState errors in detail
            foreach (var state in ModelState)
            {
                foreach (var error in state.Value.Errors)
                {
                    _logger.LogWarning($"ModelState Error in {state.Key}: {error.ErrorMessage}");
                }
            }

            _logger.LogWarning("Invalid model state for creating inventory item.");
            ViewBag.Categories = _context.Categories.ToList();
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
            ViewBag.Categories = _context.Categories.ToList();
            return View(inventoryItem);
        }

        // Edit an inventory item (POST)
        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, InventoryItem inventoryItem)
        {
            if (id != inventoryItem.InventoryItemId)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);

            // Ensure user is found
            if (user == null)
            {
                ModelState.AddModelError("", "User not found.");
                ViewBag.Categories = _context.Categories.ToList();
                return View(inventoryItem);
            }

            // Ensure category is valid
            var category = await _context.Categories.FindAsync(inventoryItem.CategoryId);
            if (category == null)
            {
                ModelState.AddModelError("CategoryId", "Invalid category selected.");
                ViewBag.Categories = _context.Categories.ToList();
                return View(inventoryItem);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingInventoryItem = await _context.InventoryItems.FindAsync(id);
                    if (existingInventoryItem == null)
                    {
                        return NotFound();
                    }

                    existingInventoryItem.Name = inventoryItem.Name;
                    existingInventoryItem.Description = inventoryItem.Description;
                    existingInventoryItem.Quantity = inventoryItem.Quantity;
                    existingInventoryItem.CategoryId = inventoryItem.CategoryId;
                    existingInventoryItem.UpdatedAt = DateTime.UtcNow;
                    existingInventoryItem.LastUpdatedById = user.Id;
                    existingInventoryItem.Category = category;
                    existingInventoryItem.LastUpdatedBy = user;

                    // Clear existing validation errors to revalidate the model with the new values
                    ModelState.Clear();
                    TryValidateModel(existingInventoryItem);

                    if (ModelState.IsValid)
                    {
                        _context.Update(existingInventoryItem);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation($"Inventory item '{inventoryItem.Name}' updated successfully.");
                        return RedirectToAction(nameof(Index));
                    }

                    // Log ModelState errors in detail
                    foreach (var state in ModelState)
                    {
                        foreach (var error in state.Value.Errors)
                        {
                            _logger.LogWarning($"ModelState Error in {state.Key}: {error.ErrorMessage}");
                        }
                    }
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
            }

            _logger.LogWarning("Invalid model state for editing inventory item.");
            ViewBag.Categories = _context.Categories.ToList();
            return View(inventoryItem);
        }

        // Delete an inventory item (GET)
        [HttpGet("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var inventoryItem = await _context.InventoryItems
                .Include(i => i.CreatedBy)
                .Include(i => i.LastUpdatedBy)
                .Include(i => i.Category)
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var inventoryItem = await _context.InventoryItems.FindAsync(id);
            _context.InventoryItems.Remove(inventoryItem);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Inventory item '{inventoryItem.Name}' deleted successfully.");
            return RedirectToAction(nameof(Index));
        }

        private bool InventoryItemExists(int id)
        {
            return _context.InventoryItems.Any(e => e.InventoryItemId == id);
        }
    }
}
