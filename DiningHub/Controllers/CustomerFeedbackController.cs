using Microsoft.AspNetCore.Mvc;
using DiningHub.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using DiningHub.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using DiningHub.Helper;

namespace DiningHub.Controllers
{
    [Authorize(Policy = "RequireAnyRole")]
    [Route("feedback")]
    public class CustomerFeedbackController : Controller
    {
        private readonly DiningHubContext _context;
        private readonly UserManager<DiningHubUser> _userManager;
        private readonly ILogger<CustomerFeedbackController> _logger;

        public CustomerFeedbackController(DiningHubContext context, UserManager<DiningHubUser> userManager, ILogger<CustomerFeedbackController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
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

            var userId = _userManager.GetUserId(User);
            var existingFeedback = await _context.Feedbacks.FirstOrDefaultAsync(f => f.OrderId == orderId && f.UserId == userId);
            if (existingFeedback != null)
            {
                return RedirectToAction("Edit", new { id = existingFeedback.FeedbackId });
            }

            var feedbackViewModel = new FeedbackViewModel
            {
                UserId = userId,
                Date = DateTime.Now,
                OrderId = orderId
            };

            ViewData["OrderDetails"] = order;
            return View(feedbackViewModel);
        }

        // Create a new feedback item (POST)
        [HttpPost("create/{orderId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int orderId, FeedbackViewModel feedbackViewModel)
        {
            var user = await _userManager.GetUserAsync(User);
            feedbackViewModel.UserId = user.Id; // Ensure UserId is set

            if (ModelState.IsValid)
            {
                var feedback = new Feedback
                {
                    UserId = feedbackViewModel.UserId,
                    Date = DateTimeHelper.GetMalaysiaTime(),
                    OrderId = feedbackViewModel.OrderId,
                    Comments = feedbackViewModel.Comments,
                    Rating = feedbackViewModel.Rating,
                    Order = await _context.Orders.Include(o => o.OrderItems).ThenInclude(oi => oi.MenuItem).FirstOrDefaultAsync(o => o.OrderId == feedbackViewModel.OrderId),
                    User = user
                };

                _context.Add(feedback);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ThankYou));
            }

            LogModelStateErrors();

            var order = await _context.Orders.Include(o => o.OrderItems).ThenInclude(oi => oi.MenuItem).FirstOrDefaultAsync(o => o.OrderId == orderId);
            ViewData["OrderDetails"] = order;
            return View(feedbackViewModel);
        }

        // Thank You page after creating feedback
        [HttpGet("thankyou")]
        public IActionResult ThankYou()
        {
            return View();
        }

        // Edit feedback item (GET)
        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var feedback = await _context.Feedbacks.Include(f => f.Order).FirstOrDefaultAsync(f => f.FeedbackId == id);
            if (feedback == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (feedback.UserId != user.Id)
            {
                return Forbid();
            }

            var feedbackViewModel = new FeedbackViewModel
            {
                FeedbackId = feedback.FeedbackId,
                UserId = feedback.UserId,
                Comments = feedback.Comments,
                Rating = feedback.Rating,
                Date = DateTimeHelper.GetMalaysiaTime(),
                OrderId = feedback.OrderId
            };

            return View(feedbackViewModel);
        }

        // Edit feedback item (POST)
        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FeedbackViewModel feedbackViewModel)
        {
            if (id != feedbackViewModel.FeedbackId)
            {
                return BadRequest();
            }

            var user = await _userManager.GetUserAsync(User);
            feedbackViewModel.UserId = user.Id; // Ensure UserId is set

            if (ModelState.IsValid)
            {
                try
                {
                    var feedback = await _context.Feedbacks.Include(f => f.Order).FirstOrDefaultAsync(f => f.FeedbackId == feedbackViewModel.FeedbackId);
                    if (feedback == null)
                    {
                        return NotFound();
                    }

                    // Check if the provided OrderId exists and is valid
                    var order = await _context.Orders.FindAsync(feedbackViewModel.OrderId);
                    if (order == null)
                    {
                        return BadRequest("The provided OrderId does not exist.");
                    }

                    feedback.Comments = feedbackViewModel.Comments;
                    feedback.Rating = feedbackViewModel.Rating;
                    feedback.Date = feedbackViewModel.Date;
                    feedback.OrderId = feedbackViewModel.OrderId;

                    _context.Update(feedback);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FeedbackExists(feedbackViewModel.FeedbackId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(ThankYou));
            }

            LogModelStateErrors();

            return View(feedbackViewModel);
        }

        private bool FeedbackExists(int id)
        {
            return _context.Feedbacks.Any(e => e.FeedbackId == id);
        }

        private void LogModelStateErrors()
        {
            foreach (var state in ModelState)
            {
                foreach (var error in state.Value.Errors)
                {
                    _logger.LogWarning($"ModelState Error in {state.Key}: {error.ErrorMessage}");
                }
            }
        }
    }
}
