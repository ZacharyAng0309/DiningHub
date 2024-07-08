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
using Microsoft.Extensions.Configuration;
using System.IO;
// Uncomment the following lines if using AWS S3
// using Amazon.S3;
// using Amazon.S3.Transfer;

namespace DiningHub.Controllers
{
    [Authorize(Policy = "RequireInternalRole")]
    [Route("manage/menu")]
    public class MenuManagementController : Controller
    {
        private readonly DiningHubContext _context;
        private readonly UserManager<DiningHubUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<MenuManagementController> _logger;

        // Uncomment the following line if using AWS S3
        // private readonly IAmazonS3 _s3Client;

        public MenuManagementController(DiningHubContext context, UserManager<DiningHubUser> userManager, IConfiguration configuration, ILogger<MenuManagementController> logger /*, IAmazonS3 s3Client*/)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
            _logger = logger;
            // Uncomment the following line if using AWS S3
            // _s3Client = s3Client;
        }

        // View all menu items
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var menuItems = await _context.MenuItems.Include(m => m.Category).ToListAsync();
            return View(menuItems);
        }

        // View details of a single menu item
        [HttpGet("details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var menuItem = await _context.MenuItems.Include(m => m.Category).Include(m => m.CreatedBy).FirstOrDefaultAsync(m => m.MenuItemId == id);
            if (menuItem == null)
            {
                return NotFound();
            }
            return View(menuItem);
        }

        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest("Invalid file type. Only JPG, JPEG, PNG, and GIF are allowed.");
            }

            var imageUrlPath = await SaveFileLocally(file);

            return Ok(new { imageUrl = imageUrlPath });
        }

        [HttpGet("create")]
        public IActionResult Create()
        {
            ViewBag.Categories = _context.Categories.ToList();
            return View();
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MenuItem menuItem)
        {
            _logger.LogInformation("Creating a new menu item.");

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                ModelState.AddModelError("", "User not found.");
                _logger.LogWarning("User not found.");
                ViewBag.Categories = _context.Categories.ToList();
                return View(menuItem);
            }

            var category = await _context.Categories.FindAsync(menuItem.CategoryId);
            if (category == null)
            {
                ModelState.AddModelError("CategoryId", "Invalid category selected.");
                _logger.LogWarning("Invalid category selected.");
                ViewBag.Categories = _context.Categories.ToList();
                return View(menuItem);
            }

            menuItem.CreatedById = user.Id;
            menuItem.LastUpdatedById = user.Id;
            menuItem.CreatedAt = DateTime.UtcNow;
            menuItem.UpdatedAt = DateTime.UtcNow;
            menuItem.Category = category;
            menuItem.CreatedBy = user;
            menuItem.LastUpdatedBy = user;

            ModelState.Clear();
            TryValidateModel(menuItem);

            if (ModelState.IsValid)
            {
                _context.Add(menuItem);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Menu item '{menuItem.Name}' created successfully.");
                return RedirectToAction(nameof(Index));
            }

            _logger.LogWarning("Invalid model state for creating menu item.");
            foreach (var state in ModelState)
            {
                foreach (var error in state.Value.Errors)
                {
                    _logger.LogWarning($"ModelState Error in {state.Key}: {error.ErrorMessage}");
                }
            }

            ViewBag.Categories = _context.Categories.ToList();
            return View(menuItem);
        }

        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var menuItem = await _context.MenuItems.Include(m => m.Category).FirstOrDefaultAsync(m => m.MenuItemId == id);
            if (menuItem == null)
            {
                return NotFound();
            }
            ViewBag.Categories = _context.Categories.ToList();
            return View(menuItem);
        }

        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MenuItem menuItem)
        {
            _logger.LogInformation($"Editing menu item with ID {id}.");

            if (id != menuItem.MenuItemId)
            {
                _logger.LogWarning($"Menu item ID mismatch. URL ID: {id}, Model ID: {menuItem.MenuItemId}");
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                ModelState.AddModelError("", "User not found.");
                _logger.LogWarning("User not found.");
                ViewBag.Categories = _context.Categories.ToList();
                return View(menuItem);
            }

            var category = await _context.Categories.FindAsync(menuItem.CategoryId);
            if (category == null)
            {
                ModelState.AddModelError("CategoryId", "Invalid category selected.");
                _logger.LogWarning("Invalid category selected.");
                ViewBag.Categories = _context.Categories.ToList();
                return View(menuItem);
            }

            var existingMenuItem = await _context.MenuItems.Include(m => m.CreatedBy).Include(m => m.LastUpdatedBy).FirstOrDefaultAsync(m => m.MenuItemId == id);
            if (existingMenuItem == null)
            {
                _logger.LogWarning($"Menu item with ID {id} not found.");
                return NotFound();
            }

            // Preserve necessary properties
            menuItem.CreatedById = existingMenuItem.CreatedById;
            menuItem.CreatedBy = existingMenuItem.CreatedBy;
            menuItem.CreatedAt = existingMenuItem.CreatedAt;
            menuItem.LastUpdatedById = user.Id;
            menuItem.LastUpdatedBy = user;
            menuItem.UpdatedAt = DateTime.UtcNow;
            menuItem.Category = category;

            // Handle ImageUrl update if needed
            if (!string.IsNullOrEmpty(menuItem.ImageUrl))
            {
                existingMenuItem.ImageUrl = menuItem.ImageUrl;
            }

            // Revalidate the model with updated properties
            ModelState.Clear();
            TryValidateModel(menuItem);

            // Log the model state errors before checking if ModelState.IsValid
            if (!ModelState.IsValid)
            {
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        _logger.LogWarning($"ModelState Error in {state.Key}: {error.ErrorMessage}");
                    }
                }

                ViewBag.Categories = _context.Categories.ToList();
                return View(menuItem);
            }

            // Update the menu item with preserved properties
            existingMenuItem.Name = menuItem.Name;
            existingMenuItem.Description = menuItem.Description;
            existingMenuItem.Price = menuItem.Price;
            existingMenuItem.CategoryId = menuItem.CategoryId;
            existingMenuItem.IsAvailable = menuItem.IsAvailable;
            existingMenuItem.UpdatedAt = menuItem.UpdatedAt;
            existingMenuItem.LastUpdatedById = menuItem.LastUpdatedById;
            existingMenuItem.LastUpdatedBy = menuItem.LastUpdatedBy;
            existingMenuItem.Category = menuItem.Category;

            try
            {
                _context.Update(existingMenuItem);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Menu item '{existingMenuItem.Name}' updated successfully.");
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MenuItemExists(existingMenuItem.MenuItemId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }


        [HttpGet("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var menuItem = await _context.MenuItems.Include(m => m.Category).FirstOrDefaultAsync(m => m.MenuItemId == id);
            if (menuItem == null)
            {
                return NotFound();
            }
            return View(menuItem);
        }

        [HttpPost("delete/{id}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var menuItem = await _context.MenuItems.FindAsync(id);
            _context.MenuItems.Remove(menuItem);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Menu item '{menuItem.Name}' deleted successfully.");
            return RedirectToAction(nameof(Index));
        }

        private bool MenuItemExists(int id)
        {
            return _context.MenuItems.Any(e => e.MenuItemId == id);
        }

        private async Task<string> SaveFileLocally(IFormFile file)
        {
            var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Img/menu");
            if (!Directory.Exists(uploads))
            {
                Directory.CreateDirectory(uploads);
            }

            // Ensure the file name is unique
            var fileName = Path.GetFileNameWithoutExtension(file.FileName);
            var extension = Path.GetExtension(file.FileName);
            var uniqueFileName = $"{fileName}_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploads, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return $"/Img/menu/{uniqueFileName}";
        }

        private void DeleteFile(string filePath)
        {
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath.TrimStart('/'));
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }

        // Uncomment this method when you want to use S3 for file uploads
        /*
        private async Task<string> UploadFileToS3(IFormFile file)
        {
            var bucketName = _configuration["AWS:BucketName"];
            var key = $"images/{file.FileName}";

            using (var newMemoryStream = new MemoryStream())
            {
                file.CopyTo(newMemoryStream);

                var uploadRequest = new TransferUtilityUploadRequest
                {
                    InputStream = newMemoryStream,
                    Key = key,
                    BucketName = bucketName,
                    CannedACL = S3CannedACL.PublicRead
                };

                var fileTransferUtility = new TransferUtility(_s3Client);
                await fileTransferUtility.UploadAsync(uploadRequest);
            }

            return $"https://{bucketName}.s3.amazonaws.com/{key}";
        }
        */
    }
}
