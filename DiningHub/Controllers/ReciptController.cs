using Microsoft.AspNetCore.Mvc;
using DiningHub.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using DiningHub.Areas.Identity.Data;

namespace DiningHub.Controllers
{
    [Authorize]
    public class ReceiptController : Controller
    {
        private readonly DiningHubContext _context;

        public ReceiptController(DiningHubContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Details(int id)
        {
            var receipt = await _context.Receipts.Include(r => r.Order)
                                                 .ThenInclude(o => o.OrderItems)
                                                 .ThenInclude(oi => oi.MenuItem)
                                                 .FirstOrDefaultAsync(r => r.ReceiptId == id);
            if (receipt == null)
            {
                return NotFound();
            }
            return View(receipt);
        }
    }
}
