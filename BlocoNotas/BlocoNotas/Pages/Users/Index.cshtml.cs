using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BlocoNotas.Data;
using BlocoNotas.Models;

namespace BlocoNotas.Pages.Users
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public IndexModel(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public IList<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
        public Dictionary<string, bool> IsAdmin { get; set; } = new();

        public async Task OnGetAsync()
        {
            Users = await _context.Users.OrderBy(u => u.UserName).ToListAsync();

            foreach (var user in Users)
            {
                IsAdmin[user.Id] = await _userManager.IsInRoleAsync(user, "Admin");
            }
        }

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
