using Microsoft.AspNetCore.Mvc;
using DiningHub.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Linq;
using DiningHub.Areas.Identity.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using System.Text.Json;
using System.Text;

namespace DiningHub.Controllers
{
    [Authorize(Policy = "RequireCustomerRole")]
    public class ShoppingCartController : Controller
    {
        private readonly DiningHubContext _context;
        private readonly UserManager<DiningHubUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<ShoppingCartController> _logger;
        private readonly IAmazonSQS _sqsClient;
        private readonly IAmazonSimpleNotificationService _snsClient;
        private readonly AWSSettings _awsSettings;

        public ShoppingCartController(DiningHubContext context, UserManager<DiningHubUser> userManager, RoleManager<IdentityRole> roleManager, ILogger<ShoppingCartController> logger, IAmazonSQS sqsClient, IAmazonSimpleNotificationService snsClient, AWSSettings awsSettings)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _sqsClient = sqsClient;
            _snsClient = snsClient;
            _awsSettings = awsSettings;
        }

        // Other methods...

        // Confirm Checkout (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmCheckout(CheckoutViewModel model)
        {
            _logger.LogInformation("ConfirmCheckout POST request received.");

            // Additional server-side validation can be added here if needed

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                _logger.LogError("User is null.");
                return RedirectToAction("Index");
            }

            var cartItems = await _context.ShoppingCartItems
                .Include(c => c.MenuItem)
                .Where(c => c.UserId == user.Id)
                .ToListAsync();

            if (!cartItems.Any())
            {
                _logger.LogWarning("No items in cart.");
                return RedirectToAction(nameof(Index));
            }

            using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var order = new Order
                    {
                        UserId = user.Id,
                        UserName = user.UserName,
                        OrderDate = DateTime.Now,
                        TotalAmount = cartItems.Sum(c => c.MenuItem.Price * c.Quantity),
                        OrderItems = cartItems.Select(c => new OrderItem
                        {
                            MenuItemId = c.MenuItemId,
                            MenuItemName = c.MenuItem.Name,
                            Quantity = c.Quantity,
                            Price = c.MenuItem.Price
                        }).ToList(),
                        PaymentMethod = model.PaymentMethod,
                        PaymentDate = DateTime.Now
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

                    await transaction.CommitAsync();

                    // Send order details to SQS
                    var orderDetails = new
                    {
                        OrderId = order.OrderId,
                        UserName = order.UserName,
                        UserEmail = user.Email,
                        OrderItems = order.OrderItems.Select(oi => new { oi.MenuItemName, oi.Quantity, oi.Price }),
                        TotalAmount = order.TotalAmount
                    };
                    var messageBody = JsonSerializer.Serialize(orderDetails);
                    var sendMessageRequest = new SendMessageRequest
                    {
                        QueueUrl = _awsSettings.SqsQueueUrl,
                        MessageBody = messageBody
                    };
                    await _sqsClient.SendMessageAsync(sendMessageRequest);

                    _logger.LogInformation("Order and receipt created successfully, and message sent to SQS.");
                    return RedirectToAction("Details", "Receipt", new { id = receipt.ReceiptId });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during checkout process.");
                    await transaction.RollbackAsync();
                    ModelState.AddModelError(string.Empty, "An error occurred while processing your order. Please try again.");
                    ViewBag.PaymentMethods = new SelectList(new List<SelectListItem>
                    {
                        new SelectListItem { Text = "Credit/Debit Card", Value = "CreditCard" },
                        new SelectListItem { Text = "Online Banking", Value = "OnlineBanking" },
                        new SelectListItem { Text = "Other", Value = "Other" }
                    }, "Value", "Text");
                    return View("Checkout", model);
                }
            }
        }
    }
}
