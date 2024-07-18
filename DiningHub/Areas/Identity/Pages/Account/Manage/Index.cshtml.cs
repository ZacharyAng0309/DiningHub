using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DiningHub.Areas.Identity.Data;

namespace DiningHub.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<DiningHubUser> _userManager;
        private readonly SignInManager<DiningHubUser> _signInManager;

        public IndexModel(UserManager<DiningHubUser> userManager, SignInManager<DiningHubUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        [BindProperty]
        public DeleteInputModel DeleteInput { get; set; }

        public IList<string> UserRoles { get; set; }

        public class InputModel
        {
            [Display(Name = "First name")]
            public string FirstName { get; set; }

            [Display(Name = "Last name")]
            public string LastName { get; set; }

            [EmailAddress]
            public string Email { get; set; }

            [Phone]
            [Display(Name = "Phone number")]
            [RegularExpression(@"\d{10,}", ErrorMessage = "The phone number must contain at least 10 digits.")]
            public string PhoneNumber { get; set; }

            [DataType(DataType.Date)]
            [Display(Name = "Date of Birth")]
            public DateTime? DateOfBirth { get; set; }
        }

        public class DeleteInputModel
        {
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Required]
            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        private async Task LoadAsync(DiningHubUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            var roles = await _userManager.GetRolesAsync(user);

            Username = userName;
            UserRoles = roles.ToList();

            Input = new InputModel
            {
                PhoneNumber = phoneNumber,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                DateOfBirth = user.DateOfBirth
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            user.FirstName = Input.FirstName;
            user.LastName = Input.LastName;
            user.DateOfBirth = Input.DateOfBirth;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                StatusMessage = "Unexpected error when trying to update profile.";
                return RedirectToPage();
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadAsync(await _userManager.GetUserAsync(User));
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var checkPasswordResult = await _userManager.CheckPasswordAsync(user, DeleteInput.Password);
            if (!checkPasswordResult)
            {
                ModelState.AddModelError(string.Empty, "Incorrect password.");
                await LoadAsync(user);
                return Page();
            }

            user.IsDeleted = true;
            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                await LoadAsync(user);
                return Page();
            }

            await _signInManager.SignOutAsync();

            return Redirect("~/");
        }
    }
}
