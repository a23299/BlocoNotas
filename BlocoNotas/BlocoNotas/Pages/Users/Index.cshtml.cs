using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BlocoNotas.Data;
using BlocoNotas.Models;

namespace BlocoNotas.Pages.Users
{
    /// <summary>
    /// PageModel for listing, deleting, and promoting users. Admin-only access.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexModel"/> class.
        /// </summary>
        /// <param name="userManager">The Identity user manager.</param>
        /// <param name="context">The application database context.</param>
        public IndexModel(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        /// <summary>
        /// Gets or sets the list of users.
        /// </summary>
        public IList<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();

        /// <summary>
        /// Gets or sets a dictionary mapping user IDs to admin status.
        /// </summary>
        public Dictionary<string, bool> IsAdmin { get; set; } = new();

        /// <summary>
        /// Handles the GET request. Loads all users and their admin status.
        /// </summary>
        public async Task OnGetAsync()
        {
            Users = await _context.Users.OrderBy(u => u.UserName).ToListAsync();

            foreach (var user in Users)
            {
                IsAdmin[user.Id] = await _userManager.IsInRoleAsync(user, "Admin");
            }
        }

        /// <summary>
        /// Handles POST delete request. Deletes a user, preventing self-deletion.
        /// </summary>
        /// <param name="id">The ID of the user to delete.</param>
        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                var currentUserId = _userManager.GetUserId(User);
                if (user.Id == currentUserId)
                {
                    ModelState.AddModelError("", "Não pode eliminar a sua própria conta.");
                    return RedirectToPage();
                }

                await _userManager.DeleteAsync(user);
            }

            return RedirectToPage();
        }

        /// <summary>
        /// Handles POST make-admin request. Promotes a user to the Admin role.
        /// </summary>
        /// <param name="id">The ID of the user to promote.</param>
        public async Task<IActionResult> OnPostMakeAdminAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                await _userManager.AddToRoleAsync(user, "Admin");
            }

            return RedirectToPage();
        }
    }
}
