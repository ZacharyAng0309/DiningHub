using Microsoft.AspNetCore.Mvc;
using DiningHub.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using DiningHub.Areas.Identity.Data;
using DiningHub.Helpers;

namespace DiningHub.Controllers
{
    [Authorize(Policy = "RequireInternalRole")]
    [Route("manage/feedback")]
    public class FeedbackManagementController : Controller
    {
        private readonly DiningHubContext _context;

        public FeedbackManagementController(DiningHubContext context)
        {
            _context = context;
        }

        // View all feedback (for managers) with search, sort, filter, and pagination
        [HttpGet("")]
        public async Task<IActionResult> Index(string searchString, string sortOrder, int? page)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["DateSortParm"] = string.IsNullOrEmpty(sortOrder) ? "date_desc" : "";
            ViewData["RatingSortParm"] = sortOrder == "Rating" ? "rating_desc" : "Rating";
            ViewData["CurrentFilter"] = searchString;

            var feedbacks = from f in _context.Feedbacks.Include(f => f.User).Include(f => f.Order)
                            select f;

            if (!string.IsNullOrEmpty(searchString))
            {
                feedbacks = feedbacks.Where(f => f.Comments.Contains(searchString) || f.User.UserName.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "date_desc":
                    feedbacks = feedbacks.OrderByDescending(f => f.Date);
                    break;
                case "Rating":
                    feedbacks = feedbacks.OrderBy(f => f.Rating);
                    break;
                case "rating_desc":
                    feedbacks = feedbacks.OrderByDescending(f => f.Rating);
                    break;
                default:
                    feedbacks = feedbacks.OrderBy(f => f.Date);
                    break;
            }

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            var paginatedFeedbacks = await PaginatedList<Feedback>.CreateAsync(feedbacks.AsNoTracking(), pageNumber, pageSize);

            return View(paginatedFeedbacks);
        }

        // View details of a single feedback item (for managers)
        [HttpGet("details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var feedback = await _context.Feedbacks.Include(f => f.User).FirstOrDefaultAsync(f => f.FeedbackId == id);
            if (feedback == null)
            {
                return NotFound();
            }
            return View(feedback);
        }

        // Delete a feedback item (GET)
        [HttpGet("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var feedback = await _context.Feedbacks.Include(f => f.User).FirstOrDefaultAsync(f => f.FeedbackId == id);
            if (feedback == null)
            {
                return NotFound();
            }
            return View(feedback);
        }

        // Delete a feedback item (POST)
        [HttpPost("delete/{id}")]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback == null)
            {
                return NotFound();
            }

            try
            {
                _context.Feedbacks.Remove(feedback);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Feedback deleted successfully.";
            }
            catch (DbUpdateException ex)
            {
                // Log the error (uncomment ex variable name and write a log.)
                TempData["ErrorMessage"] = "Unable to delete feedback. Try again, and if the problem persists see your system administrator.";
                // You might want to log the exception here
            }

            return RedirectToAction(nameof(Index));
        }

        private bool FeedbackExists(int id)
        {
            return _context.Feedbacks.Any(e => e.FeedbackId == id);
        }
    }
}
