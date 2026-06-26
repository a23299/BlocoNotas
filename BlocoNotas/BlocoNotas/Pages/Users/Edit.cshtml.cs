using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BlocoNotas.Models;

namespace BlocoNotas.Pages.Users
{
    /// <summary>
    /// PageModel for editing a user's profile. Admin-only access.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="EditModel"/> class.
        /// </summary>
        /// <param name="userManager">The Identity user manager.</param>
        public EditModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        /// <summary>
        /// Gets or sets the input model for the edit form.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; } = new();

        /// <summary>
        /// Input model for the user edit form.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            /// Gets or sets the user ID.
            /// </summary>
            public string Id { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the username.
            /// </summary>
            public string UserName { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the email address.
            /// </summary>
            public string Email { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the optional new password.
            /// </summary>
            public string? Password { get; set; }
        }

        /// <summary>
        /// Handles the GET request. Loads the user data into the edit form.
        /// </summary>
        /// <param name="id">The ID of the user to edit.</param>
        public async Task<IActionResult> OnGetAsync(string? id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            Input = new InputModel
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty
            };

            return Page();
        }

        /// <summary>
        /// Handles the POST request. Updates the user's profile and optionally resets the password.
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var user = await _userManager.FindByIdAsync(Input.Id);
            if (user == null) return NotFound();

            user.UserName = Input.UserName;
            user.Email = Input.Email;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
                return Page();
            }

            if (!string.IsNullOrWhiteSpace(Input.Password))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                result = await _userManager.ResetPasswordAsync(user, token, Input.Password);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                        ModelState.AddModelError("", error.Description);
                    return Page();
                }
            }

            return RedirectToPage("./Index");
        }
    }
}
