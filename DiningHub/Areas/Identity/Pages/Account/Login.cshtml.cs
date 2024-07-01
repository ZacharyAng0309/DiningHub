#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using DiningHub.Areas.Identity.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace DiningHub.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<DiningHubUser> _signInManager;
        private readonly UserManager<DiningHubUser> _userManager;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(SignInManager<DiningHubUser> signInManager, UserManager<DiningHubUser> userManager, ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                // Normalize email to ensure consistency
                var normalizedEmail = _userManager.NormalizeEmail(Input.Email);
                _logger.LogInformation("Normalized email: {NormalizedEmail}", normalizedEmail);

                // Check if the user exists
                var user = await _userManager.FindByEmailAsync(normalizedEmail);
                if (user != null)
                {
                    _logger.LogInformation("User found: {Email}", normalizedEmail);

                    // Check if the user is locked out
                    if (await _userManager.IsLockedOutAsync(user))
                    {
                        _logger.LogWarning("User account locked out.");
                        return RedirectToPage("./Lockout");
                    }

                    // Validate the password
                    var passwordValid = await _userManager.CheckPasswordAsync(user, Input.Password);
                    if (passwordValid)
                    {
                        _logger.LogInformation("Password validation succeeded for user: {Email}", normalizedEmail);

                        // Check if the user is confirmed if required
                        if (!_userManager.Options.SignIn.RequireConfirmedAccount || await _userManager.IsEmailConfirmedAsync(user))
                        {
                            var result = await _signInManager.PasswordSignInAsync(user, Input.Password, Input.RememberMe, lockoutOnFailure: true);

                            LogSignInResult(result);

                            if (result.Succeeded)
                            {
                                _logger.LogInformation("User logged in.");
                                return LocalRedirect(returnUrl);
                            }
                            if (result.RequiresTwoFactor)
                            {
                                _logger.LogInformation("User requires two-factor authentication.");
                                return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                            }
                            if (result.IsLockedOut)
                            {
                                _logger.LogWarning("User account locked out.");
                                return RedirectToPage("./Lockout");
                            }
                            else
                            {
                                _logger.LogWarning("Invalid login attempt.");
                                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                                return Page();
                            }
                        }
                        else
                        {
                            _logger.LogWarning("User email not confirmed: {Email}", normalizedEmail);
                            ModelState.AddModelError(string.Empty, "Email not confirmed. Please confirm your email to log in.");
                            return Page();
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Invalid password for user: {Email}", normalizedEmail);
                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                        return Page();
                    }
                }
                else
                {
                    _logger.LogWarning("User not found: {Email}", normalizedEmail);
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private void LogSignInResult(Microsoft.AspNetCore.Identity.SignInResult result)
        {
            if (result.Succeeded)
            {
                _logger.LogInformation("SignInResult: Success");
            }
            else if (result.RequiresTwoFactor)
            {
                _logger.LogInformation("SignInResult: RequiresTwoFactor");
            }
            else if (result.IsLockedOut)
            {
                _logger.LogInformation("SignInResult: IsLockedOut");
            }
            else
            {
                _logger.LogInformation("SignInResult: Failed");
            }
        }
    }
}
