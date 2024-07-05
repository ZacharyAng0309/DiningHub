using Microsoft.AspNetCore.Mvc;
using DiningHub.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Linq;
using DiningHub.Areas.Identity.Data;

namespace DiningHub.Controllers
{
    [Authorize(Policy ="RequireCustomerRole")]
    public class ShoppingCartController : Controller
    {
        private readonly DiningHubContext _context;

        public ShoppingCartController(DiningHubContext context)
        {
            _context = context;
        }

        // View the shopping cart
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cartItems = await _context.ShoppingCartItems.Include(c => c.MenuItem).Where(c => c.UserId == userId).ToListAsync();
            return View(cartItems);
        }

        // Add item to the cart (GET)
        public async Task<IActionResult> AddToCart(int menuItemId)
        {
            var menuItem = await _context.MenuItems.FindAsync(menuItemId);
            if (menuItem == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cartItem = await _context.ShoppingCartItems.FirstOrDefaultAsync(c => c.MenuItemId == menuItemId && c.UserId == userId);

            if (cartItem == null)
            {
                cartItem = new ShoppingCartItem
                {
                    MenuItemId = menuItemId,
                    Quantity = 1,
                    UserId = userId
                };
                _context.ShoppingCartItems.Add(cartItem);
            }
            else
            {
                cartItem.Quantity++;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Remove item from the cart (GET)
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            var cartItem = await _context.ShoppingCartItems.FindAsync(cartItemId);
            if (cartItem == null)
            {
                return NotFound();
            }

            _context.ShoppingCartItems.Remove(cartItem);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Update the quantity of an item in the cart (POST)
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            var cartItem = await _context.ShoppingCartItems.FindAsync(cartItemId);
            if (cartItem == null)
            {
                return NotFound();
            }

            if (quantity > 0)
            {
                cartItem.Quantity = quantity;
                _context.ShoppingCartItems.Update(cartItem);
            }
            else
            {
                _context.ShoppingCartItems.Remove(cartItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Checkout (GET)
        public async Task<IActionResult> Checkout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cartItems = await _context.ShoppingCartItems.Include(c => c.MenuItem).Where(c => c.UserId == userId).ToListAsync();

            if (!cartItems.Any())
            {
                return RedirectToAction(nameof(Index));
            }

            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                TotalAmount = cartItems.Sum(c => c.MenuItem.Price * c.Quantity),
                OrderItems = cartItems.Select(c => new OrderItem
                {
                    MenuItemId = c.MenuItemId,
                    Quantity = c.Quantity,
                    Price = c.MenuItem.Price
                }).ToList()
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            var receipt = new Receipt
            {
                OrderId = order.OrderId,
                TotalAmount = order.TotalAmount,
                DateIssued = DateTime.Now
            };

            _context.Receipts.Add(receipt);
            _context.ShoppingCartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Receipt", new { id = receipt.ReceiptId });
        }
    }
}
