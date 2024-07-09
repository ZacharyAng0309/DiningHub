using Microsoft.AspNetCore.Mvc;
using DiningHub.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System;
using DiningHub.Areas.Identity.Data;

namespace DiningHub.Controllers
{
    [Authorize(Policy = "RequireAnyRole")]
    [Route("order")]
    public class CustomerOrderController : Controller
    {
        private readonly DiningHubContext _context;
        private readonly UserManager<DiningHubUser> _userManager;

        public CustomerOrderController(DiningHubContext context, UserManager<DiningHubUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // View order history for a customer
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var orders = await _context.Orders
                .Where(o => o.UserId == user.Id)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
                .ToListAsync();

            var feedbacks = await _context.Feedbacks.Where(f => f.UserId == user.Id).ToListAsync();

            var model = orders.Select(order => new CustomerOrderViewModel
            {
                Order = order,
                HasFeedback = feedbacks.Any(f => f.OrderId == order.OrderId),
                CanProvideFeedback = order.OrderDate >= DateTime.Now.AddDays(-5),
                FeedbackId = feedbacks.FirstOrDefault(f => f.OrderId == order.OrderId)?.FeedbackId
            });

            return View(model);
        }

        // View details of a single order (for customers)
        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.OrderId == id && o.UserId == user.Id);

            if (order == null)
            {
                return NotFound();
            }

            var feedback = await _context.Feedbacks.FirstOrDefaultAsync(f => f.OrderId == id && f.UserId == user.Id);

            var model = new CustomerOrderDetailsViewModel
            {
                Order = order,
                Feedback = feedback,
                CanProvideFeedback = order.OrderDate >= DateTime.Now.AddDays(-5) && feedback == null
            };

            return View(model);
        }
    }
}
