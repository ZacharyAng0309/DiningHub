﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using DiningHub.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using DiningHub.Helper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace DiningHub.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<DiningHubUser> _signInManager;
        private readonly UserManager<DiningHubUser> _userManager;
        private readonly IUserStore<DiningHubUser> _userStore;
        private readonly IUserEmailStore<DiningHubUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<DiningHubUser> userManager,
            IUserStore<DiningHubUser> userStore,
            SignInManager<DiningHubUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Required]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Required]
            [Phone]
            [Display(Name = "Phone Number")]
            [RegularExpression(@"\d{10,}", ErrorMessage = "The phone number must contain at least 10 digits.")]
            public string PhoneNumber { get; set; }

            // Optional: Role selection during registration
            [Display(Name = "Role")]
            public string Role { get; set; } = "Customer";  // Default role
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                try
                {
                    var user = CreateUser();

                    await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                    await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                    user.FirstName = Input.FirstName;
                    user.LastName = Input.LastName;
                    user.PhoneNumber = Input.PhoneNumber;
                    user.EmailConfirmed = true;
                    user.CreatedAt = DateTimeHelper.GetMalaysiaTime();
                    user.UpdatedAt = DateTimeHelper.GetMalaysiaTime();

                    var result = await _userManager.CreateAsync(user, Input.Password);

                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User created a new account with password.");

                        // Assign role to the user
                        if (!string.IsNullOrEmpty(Input.Role))
                        {
                            await _userManager.AddToRoleAsync(user, Input.Role);
                        }

                        // Directly sign in the user without email confirmation
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number == 2627)
                {
                    // Handle duplicate key exception
                    ModelState.AddModelError(string.Empty, "An account with this email address already exists.");
                    _logger.LogError($"Duplicate key error: {ex.Message}");
                }
                catch (Exception ex)
                {
                    // Handle other exceptions
                    ModelState.AddModelError(string.Empty, "A user with this email address already exists but is deactivated. Please contact support to reactivate the account.");
                    _logger.LogError($"Unexpected error: {ex.Message}");
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }


        private DiningHubUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<DiningHubUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(DiningHubUser)}'. " +
                    $"Ensure that '{nameof(DiningHubUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<DiningHubUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<DiningHubUser>)_userStore;
        }
    }
}
