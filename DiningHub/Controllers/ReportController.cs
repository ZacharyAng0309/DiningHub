using Microsoft.AspNetCore.Mvc;
using DiningHub.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DiningHub.Areas.Identity.Data;

namespace DiningHub.Controllers
{
    public class ReportController : Controller
    {
        private readonly DiningHubContext _context;

        public ReportController(DiningHubContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Example: Generate a sales report
            var salesReport = await _context.Orders
                                            .Include(o => o.OrderItems)
                                            .ThenInclude(oi => oi.MenuItem)
                                            .ToListAsync();

            // Logic to format and present the sales report
            // e.g., grouping by date, calculating totals, etc.

            return View(salesReport);
        }
    }
}
