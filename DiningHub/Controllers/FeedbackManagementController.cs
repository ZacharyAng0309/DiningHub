using Microsoft.AspNetCore.Mvc;
using DiningHub.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System;
using DiningHub.Areas.Identity.Data;

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

        // View all feedback (for managers)
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var feedbacks = await _context.Feedbacks.Include(f => f.User).ToListAsync();
            return View(feedbacks);
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
            _context.Feedbacks.Remove(feedback);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FeedbackExists(int id)
        {
            return _context.Feedbacks.Any(e => e.FeedbackId == id);
        }
    }
}
