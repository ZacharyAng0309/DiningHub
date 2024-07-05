using Microsoft.AspNetCore.Mvc;
using DiningHub.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using DiningHub.Areas.Identity.Data;

namespace DiningHub.Controllers
{
    [Authorize(Policy = "RequireInternalRole")]
    [Route("manage/order")]
    public class OrderManagementController : Controller
    {
        private readonly DiningHubContext _context;

        public OrderManagementController(DiningHubContext context)
        {
            _context = context;
        }

        // View all orders (for managers)
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders.Include(o => o.User).Include(o => o.OrderItems).ThenInclude(oi => oi.MenuItem).ToListAsync();
            return View(orders);
        }

        // View details of a single order (for managers)
        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
    }
}
