using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using DiningHub.Models;
using DiningHub.Areas.Identity.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace DiningHub.Controllers
{
    [Authorize]
    public class ReceiptController : Controller
    {
        private readonly DiningHubContext _context;
        private readonly UserManager<DiningHubUser> _userManager;

        public ReceiptController(DiningHubContext context, UserManager<DiningHubUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Details(int id)
        {
            var receipt = await _context.Receipts
                .Include(r => r.Order)
                .ThenInclude(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
                .FirstOrDefaultAsync(r => r.ReceiptId == id);

            if (receipt == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user);

            bool isAdmin = roles.Contains("Admin");
            bool isStaff = roles.Contains("Staff");
            bool isOrderOwner = receipt.Order.UserId == userId;

            if (!isAdmin && !isStaff && !isOrderOwner)
            {
                return Forbid();
            }

            return View(receipt);
        }
    }
}
