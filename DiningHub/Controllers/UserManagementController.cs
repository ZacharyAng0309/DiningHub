﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using DiningHub.Areas.Identity.Data;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DiningHub.Models;
using DiningHub.Helper;
using X.PagedList;

namespace DiningHub.Controllers
{
    [Authorize(Policy = "RequireManagerRole")]
    [Route("manage/user")]
    public class UserManagementController : Controller
    {
        private readonly UserManager<DiningHubUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<UserManagementController> _logger;

        public UserManagementController(UserManager<DiningHubUser> userManager, RoleManager<IdentityRole> roleManager, ILogger<UserManagementController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(string searchString, string sortOrder, int? page)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["EmailSortParm"] = sortOrder == "Email" ? "email_desc" : "Email";
            ViewData["CurrentFilter"] = searchString;

            // Fetch users with AsNoTracking and ToListAsync
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

            var users = await usersQuery.AsNoTracking().ToListAsync();

            // Fetch roles in a batch
            var userRoles = new Dictionary<string, IList<string>>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles[user.Id] = roles;
            }

            int pageSize = 10;
            int pageNumber = (page ?? 1);

            var model = new ManageUsersViewModel
            {
                Users = users.ToPagedList(pageNumber, pageSize),
                UserRoles = userRoles
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
                // Check if a user with the given email already exists
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "A user with this email address already exists.");
                    _logger.LogWarning($"A user with the email {model.Email} already exists.");
                    return View(model);
                }

                var user = new DiningHubUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
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
                        return RedirectToAction(nameof(Index));
                    }
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }
    }
}
