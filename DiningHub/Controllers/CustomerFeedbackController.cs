using Microsoft.AspNetCore.Mvc;
using DiningHub.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using DiningHub.Areas.Identity.Data;
using System;

namespace DiningHub.Controllers
{
    [Authorize(Roles = "Customer")]
    [Route("feedback")]
    public class CustomerFeedbackController : Controller
    {
        private readonly DiningHubContext _context;

        public CustomerFeedbackController(DiningHubContext context)
        {
            _context = context;
        }

        // Create a new feedback item (GET)
        [HttpGet("create/{orderId}")]
        public async Task<IActionResult> Create(int orderId)
        {
            var order = await _context.Orders.Include(o => o.OrderItems).ThenInclude(oi => oi.MenuItem).FirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order == null)
            {
                return NotFound();
            }

            var feedback = new Feedback
            {
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                Date = DateTime.Now
            };

            ViewData["OrderId"] = orderId;
            ViewData["OrderDetails"] = order;
            return View(feedback);
        }

        // Create a new feedback item (POST)
        [HttpPost("create/{orderId}")]
        public async Task<IActionResult> Create(int orderId, Feedback feedback)
        {
            if (ModelState.IsValid)
            {
                feedback.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                feedback.Date = DateTime.Now;
                _context.Add(feedback);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ThankYou));
            }
            return View(feedback);
        }

        // Thank You page after creating feedback
        [HttpGet("thankyou")]
        public IActionResult ThankYou()
        {
            return View();
        }
    }
}
