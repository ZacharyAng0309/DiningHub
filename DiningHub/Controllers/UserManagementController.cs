using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using DiningHub.Areas.Identity.Data;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using DiningHub.Models;
using DiningHub.Helper;
using Microsoft.Data.SqlClient;
using DiningHub.Helpers;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

namespace DiningHub.Controllers
{
    [Authorize(Policy = "RequireManagerRole")]
    [Route("manage/user")]
    public class UserManagementController : Controller
    {
        private readonly UserManager<DiningHubUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<UserManagementController> _logger;
        private readonly IAmazonSimpleNotificationService _snsClient;
        private readonly string _staffSnsTopicArn;

        public UserManagementController(UserManager<DiningHubUser> userManager, RoleManager<IdentityRole> roleManager, ILogger<UserManagementController> logger, IAmazonSimpleNotificationService snsClient)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _snsClient = snsClient;
            _staffSnsTopicArn = Environment.GetEnvironmentVariable("STAFF_SNS_TOPIC_ARN")
                ?? throw new InvalidOperationException("STAFF_SNS_TOPIC_ARN is not configured.");
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(string searchString, string sortOrder, int? page)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["EmailSortParm"] = sortOrder == "Email" ? "email_desc" : "Email";
            ViewData["CurrentFilter"] = searchString;

            var usersQuery = _userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                usersQuery = usersQuery.Where(u => u.UserName.Contains(searchString)
                                                   || u.Email.Contains(searchString)
                                                   || u.FirstName.Contains(searchString)
                                                   || u.LastName.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    usersQuery = usersQuery.OrderByDescending(u => u.UserName);
                    break;
                case "Email":
                    usersQuery = usersQuery.OrderBy(u => u.Email);
                    break;
                case "email_desc":
                    usersQuery = usersQuery.OrderByDescending(u => u.Email);
                    break;
                default:
                    usersQuery = usersQuery.OrderBy(u => u.UserName);
                    break;
            }

            int pageSize = 10;
            int pageNumber = (page ?? 1);

            var paginatedUsers = await PaginatedList<DiningHubUser>.CreateAsync(usersQuery.AsNoTracking(), pageNumber, pageSize);

            var userRoles = new Dictionary<string, IList<string>>();
            foreach (var user in paginatedUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles[user.Id] = roles;
            }

            var model = new ManageUsersViewModel
            {
                Users = paginatedUsers,
                UserRoles = userRoles,
                PageNumber = paginatedUsers.PageIndex,
                TotalPages = paginatedUsers.TotalPages,
                HasPreviousPage = paginatedUsers.HasPreviousPage,
                HasNextPage = paginatedUsers.HasNextPage
            };

            return View(model);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Details(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            var roles = await _userManager.GetRolesAsync(user);
            ViewData["Roles"] = roles;
            return View(user);
        }

        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            var roles = await _userManager.GetRolesAsync(user);
            ViewData["Roles"] = roles;
            return View(user);
        }

        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,UserName,Email,FirstName,LastName,PhoneNumber")] DiningHubUser user)
        {
            if (id != user.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogError($"ModelState Error: {error.ErrorMessage}");
                }
                return View(user);
            }

            var existingUser = await _userManager.FindByIdAsync(user.Id);
            if (existingUser == null)
            {
                return NotFound();
            }

            var userWithSameUsername = await _userManager.Users
                .FirstOrDefaultAsync(u => u.UserName == user.UserName && u.Id != user.Id);
            if (userWithSameUsername != null)
            {
                ModelState.AddModelError("UserName", "A user with this username already exists.");
                _logger.LogWarning($"A user with the username {user.UserName} already exists.");
                return View(user);
            }

            var userWithSameEmail = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Email == user.Email && u.Id != user.Id);
            if (userWithSameEmail != null)
            {
                ModelState.AddModelError("Email", "A user with this email address already exists.");
                _logger.LogWarning($"A user with the email {user.Email} already exists.");
                return View(user);
            }

            _logger.LogInformation($"Updating user: {user.UserName}");

            existingUser.UserName = user.UserName;
            existingUser.Email = user.Email;
            existingUser.FirstName = user.FirstName;
            existingUser.LastName = user.LastName;
            existingUser.PhoneNumber = user.PhoneNumber;
            existingUser.UpdatedAt = DateTimeHelper.GetMalaysiaTime();

            var result = await _userManager.UpdateAsync(existingUser);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
                _logger.LogError($"Update Error: {error.Description}");
            }

            return View(user);
        }

        [HttpGet("delete/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        [HttpPost("delete/{id}")]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Soft delete by setting IsDeleted to true
            user.IsDeleted = true;
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                // Unsubscribe the staff from SNS
                await UnsubscribeStaffAsync(user.Email);
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(user);
        }

        [HttpGet("manage-roles/{id}")]
        public async Task<IActionResult> ManageRoles(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _roleManager.Roles.ToListAsync();
            var userRoles = await _userManager.GetRolesAsync(user);
            var model = new ManageRolesViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                AvailableRoles = roles,
                UserRoles = userRoles,
            };

            return View(model);
        }

        [HttpPost("manage-roles/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageRoles(ManageRolesViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var selectedRoles = model.SelectedRoles ?? new string[] { };

            var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Failed to add roles");
                return View(model);
            }

            result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Failed to remove roles");
                return View(model);
            }

            // Subscribe or unsubscribe from SNS topic based on roles
            if (selectedRoles.Contains("Staff"))
            {
                await SubscribeStaffAsync(user.Email);
            }
            else
            {
                await UnsubscribeStaffAsync(user.Email);
            }

            return RedirectToAction(nameof(Details), new { id = user.Id });
        }

        [HttpGet("add-staff")]
        public IActionResult AddStaff()
        {
            return View();
        }

        [HttpPost("add-staff")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStaff(AddStaffViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Check if a user with the given email already exists
                    var existingUser = await _userManager.FindByEmailAsync(model.Email);
                    if (existingUser != null)
                    {
                        if (existingUser.IsDeleted)
                        {
                            TempData["AlertMessage"] = "A user with this email address already exists but is deactivated. Please contact support to reactivate the account.";
                            _logger.LogWarning($"A user with the email {model.Email} already exists but is deactivated.");
                        }
                        else
                        {
                            TempData["AlertMessage"] = "A user with this email address already exists.";
                            _logger.LogWarning($"A user with the email {model.Email} already exists.");
                        }
                        _logger.LogInformation("Redirecting to AddStaff due to existing user.");
                        return RedirectToAction(nameof(AddStaff)); // Ensure redirect to same action to display TempData
                    }

                    var user = new DiningHubUser
                    {
                        UserName = model.Email,
                        Email = model.Email,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        PhoneNumber = model.PhoneNumber,
                        EmailConfirmed = true, // Set to true if email confirmation is not required
                        CreatedAt = DateTimeHelper.GetMalaysiaTime(),
                        UpdatedAt = DateTimeHelper.GetMalaysiaTime()
                    };

                    var result = await _userManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        result = await _userManager.AddToRoleAsync(user, "Staff");
                        if (result.Succeeded)
                        {
                            _logger.LogInformation($"Staff user {model.Email} created successfully.");
                            // Subscribe the new staff member to the SNS topic
                            await SubscribeStaffAsync(user.Email);
                            return RedirectToAction(nameof(Index));
                        }
                        else
                        {
                            _logger.LogError("Failed to add role 'Staff' to user.");
                        }
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            // Check if the error is related to the password and add to ModelState
                            if (error.Code.StartsWith("Password"))
                            {
                                ModelState.AddModelError("Password", error.Description);
                            }
                            else
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                            _logger.LogError($"Error creating user: {error.Description}");
                        }
                    }
                }
                catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number == 2627)
                {
                    // Handle duplicate key exception
                    TempData["AlertMessage"] = "An account with this email address already exists.";
                    _logger.LogError($"Duplicate key error: {ex.Message}");
                }
                catch (Exception ex)
                {
                    // Handle other exceptions
                    TempData["AlertMessage"] = "An error occurred while creating the account. Please try again later.";
                    _logger.LogError($"Unexpected error: {ex.Message}");
                }
            }
            else
            {
                TempData["AlertMessage"] = "Please correct the errors in the form.";
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogError($"ModelState Error: {error.ErrorMessage}");
                }
            }

            _logger.LogInformation("Redirecting to AddStaff due to form errors.");
            return View(model); // Ensure the view is returned with model to display validation errors
        }

        private async Task SubscribeStaffAsync(string email)
        {
            try
            {
                var subscribeRequest = new SubscribeRequest
                {
                    TopicArn = _staffSnsTopicArn,
                    Protocol = "email",
                    Endpoint = email,
                    ReturnSubscriptionArn = true // Automatically confirm the subscription
                };
                var subscribeResponse = await _snsClient.SubscribeAsync(subscribeRequest);
                _logger.LogInformation($"Staff {email} subscribed to SNS topic with SubscriptionArn: {subscribeResponse.SubscriptionArn}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error subscribing staff {email} to SNS topic: {ex.Message}");
            }
        }

        private async Task UnsubscribeStaffAsync(string email)
        {
            try
            {
                var listSubscriptionsResponse = await _snsClient.ListSubscriptionsByTopicAsync(_staffSnsTopicArn);
                var subscription = listSubscriptionsResponse.Subscriptions.FirstOrDefault(s => s.Endpoint == email);
                if (subscription != null)
                {
                    await _snsClient.UnsubscribeAsync(subscription.SubscriptionArn);
                    _logger.LogInformation($"Staff {email} unsubscribed from SNS topic with SubscriptionArn: {subscription.SubscriptionArn}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error unsubscribing staff {email} from SNS topic: {ex.Message}");
            }
        }
    }
}
