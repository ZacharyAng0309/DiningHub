using Microsoft.AspNetCore.Mvc;
using DiningHub.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using DiningHub.Areas.Identity.Data;
using DiningHub.Helpers;

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

        [HttpGet("")]
        public async Task<IActionResult> Index(string searchString, string sortOrder, int? page)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["OrderDateSortParm"] = string.IsNullOrEmpty(sortOrder) ? "date_desc" : "";
            ViewData["TotalAmountSortParm"] = sortOrder == "TotalAmount" ? "amount_desc" : "TotalAmount";
            ViewData["CurrentFilter"] = searchString;

            var orders = from o in _context.Orders
                         .Include(o => o.User)
                         .Include(o => o.OrderItems)
                         .ThenInclude(oi => oi.MenuItem)
                         select o;

            if (!string.IsNullOrEmpty(searchString))
            {
                orders = orders.Where(o => o.User.UserName.Contains(searchString)
                                         || o.OrderItems.Any(oi => oi.MenuItem.Name.Contains(searchString)));
            }

            switch (sortOrder)
            {
                case "date_desc":
                    orders = orders.OrderByDescending(o => o.OrderDate);
                    break;
                case "TotalAmount":
                    orders = orders.OrderBy(o => o.TotalAmount);
                    break;
                case "amount_desc":
                    orders = orders.OrderByDescending(o => o.TotalAmount);
                    break;
                default:
                    orders = orders.OrderBy(o => o.OrderDate);
                    break;
            }

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            var pagedOrders = await PaginatedList<Order>.CreateAsync(orders.AsNoTracking(), pageNumber, pageSize);

            return View(pagedOrders);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
                .Include(o => o.User)
                .Include(o => o.Feedback)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
    }
}
