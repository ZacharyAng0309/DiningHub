using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DiningHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Rendering;
using DiningHub.Helpers;
using DiningHub.Areas.Identity.Data;
using DiningHub.Helper;

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

        // View all inventory items with search, filter, sort, and pagination
        [HttpGet("")]
        public async Task<IActionResult> Index(string searchString, int? categoryId, string sortOrder, int? page)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["QuantitySortParm"] = sortOrder == "Quantity" ? "quantity_desc" : "Quantity";
            ViewData["LastUpdatedSortParm"] = sortOrder == "LastUpdated" ? "last_updated_desc" : "LastUpdated";
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentCategory"] = categoryId;

            ViewBag.Categories = await _context.Categories.ToListAsync();

            var inventoryItems = from i in _context.InventoryItems.Include(i => i.Category).Include(i => i.CreatedBy).Include(i => i.LastUpdatedBy)
                                 select i;

            if (!string.IsNullOrEmpty(searchString))
            {
                inventoryItems = inventoryItems.Where(i => i.Name.Contains(searchString) || i.Description.Contains(searchString));
            }

            if (categoryId.HasValue)
            {
                inventoryItems = inventoryItems.Where(i => i.CategoryId == categoryId.Value);
            }

            switch (sortOrder)
            {
                case "name_desc":
                    inventoryItems = inventoryItems.OrderByDescending(i => i.Name);
                    break;
                case "Quantity":
                    inventoryItems = inventoryItems.OrderBy(i => i.Quantity);
                    break;
                case "quantity_desc":
                    inventoryItems = inventoryItems.OrderByDescending(i => i.Quantity);
                    break;
                case "LastUpdated":
                    inventoryItems = inventoryItems.OrderBy(i => i.UpdatedAt);
                    break;
                case "last_updated_desc":
                    inventoryItems = inventoryItems.OrderByDescending(i => i.UpdatedAt);
                    break;
                default:
                    inventoryItems = inventoryItems.OrderBy(i => i.Name);
                    break;
            }

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            var pagedInventoryItems = await PaginatedList<InventoryItem>.CreateAsync(inventoryItems.AsNoTracking(), pageNumber, pageSize);

            return View(pagedInventoryItems);
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
            ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name");
            return View();
        }

        // Create a new inventory item (POST)
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InventoryItem inventoryItem)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                ModelState.AddModelError("", "User not found.");
                ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name");
                return View(inventoryItem);
            }

            var trimmedName = inventoryItem.Name.Trim().ToLower();
            var existingInventoryItem = await _context.InventoryItems
                .FirstOrDefaultAsync(i => i.Name.Trim().ToLower() == trimmedName);

            if (existingInventoryItem != null)
            {
                ModelState.AddModelError("Name", "An inventory item with this name already exists.");
                _logger.LogWarning("An inventory item with this name already exists.");
                ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name");
                return View(inventoryItem);
            }

            var category = await _context.Categories.FindAsync(inventoryItem.CategoryId);
            if (category == null)
            {
                ModelState.AddModelError("CategoryId", "Invalid category selected.");
                ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name");
                return View(inventoryItem);
            }

            inventoryItem.CreatedById = user.Id;
            inventoryItem.LastUpdatedById = user.Id;
            inventoryItem.CreatedAt = DateTimeHelper.GetMalaysiaTime();
            inventoryItem.UpdatedAt = DateTimeHelper.GetMalaysiaTime();
            inventoryItem.Category = category;
            inventoryItem.CreatedBy = user;
            inventoryItem.LastUpdatedBy = user;

            ModelState.Clear();
            TryValidateModel(inventoryItem);

            if (ModelState.IsValid)
            {
                _context.Add(inventoryItem);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Inventory item '{inventoryItem.Name}' created successfully.");
                return RedirectToAction(nameof(Index));
            }

            foreach (var state in ModelState)
            {
                foreach (var error in state.Value.Errors)
                {
                    _logger.LogWarning($"ModelState Error in {state.Key}: {error.ErrorMessage}");
                }
            }

            _logger.LogWarning("Invalid model state for creating inventory item.");
            ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name");
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

            ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name", inventoryItem.CategoryId);
            return View(inventoryItem);
        }


        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, InventoryItem inventoryItem)
        {
            _logger.LogInformation($"Editing inventory item with ID {id}.");

            if (id != inventoryItem.InventoryItemId)
            {
                _logger.LogWarning($"Inventory item ID mismatch. URL ID: {id}, Model ID: {inventoryItem.InventoryItemId}");
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                ModelState.AddModelError("", "User not found.");
                _logger.LogWarning("User not found.");
                ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name", inventoryItem.CategoryId);
                return View(inventoryItem);
            }

            var trimmedName = inventoryItem.Name.Trim().ToLower();
            var existingInventoryItem = await _context.InventoryItems
                .FirstOrDefaultAsync(i => i.Name.Trim().ToLower() == trimmedName && i.InventoryItemId != id);

            if (existingInventoryItem != null)
            {
                ModelState.AddModelError("Name", "An inventory item with this name already exists.");
                _logger.LogWarning("An inventory item with this name already exists.");
                ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name", inventoryItem.CategoryId);
                return View(inventoryItem);
            }

            var category = await _context.Categories.FindAsync(inventoryItem.CategoryId);
            if (category == null)
            {
                ModelState.AddModelError("CategoryId", "Invalid category selected.");
                _logger.LogWarning("Invalid category selected.");
                ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name", inventoryItem.CategoryId);
                return View(inventoryItem);
            }

            var existingInventoryItemInDb = await _context.InventoryItems.FindAsync(id);
            if (existingInventoryItemInDb == null)
            {
                _logger.LogWarning($"Inventory item with ID {id} not found.");
                return NotFound();
            }

            existingInventoryItemInDb.Name = inventoryItem.Name;
            existingInventoryItemInDb.Description = inventoryItem.Description;
            existingInventoryItemInDb.Quantity = inventoryItem.Quantity;
            existingInventoryItemInDb.CategoryId = inventoryItem.CategoryId;
            existingInventoryItemInDb.UpdatedAt = DateTimeHelper.GetMalaysiaTime();
            existingInventoryItemInDb.LastUpdatedById = user.Id;
            existingInventoryItemInDb.Category = category;
            existingInventoryItemInDb.LastUpdatedBy = user;

            ModelState.Clear();
            TryValidateModel(existingInventoryItemInDb);

            if (!ModelState.IsValid)
            {
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        _logger.LogWarning($"ModelState Error in {state.Key}: {error.ErrorMessage}");
                    }
                }

                ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name", inventoryItem.CategoryId);
                return View(inventoryItem);
            }

            try
            {
                _context.Update(existingInventoryItemInDb);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Inventory item '{existingInventoryItemInDb.Name}' updated successfully.");
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InventoryItemExists(existingInventoryItemInDb.InventoryItemId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
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
