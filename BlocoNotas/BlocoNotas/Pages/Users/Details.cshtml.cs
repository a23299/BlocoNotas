using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BlocoNotas.Data;
using BlocoNotas.Models;

namespace BlocoNotas.Pages.Users
{
    /// <summary>
    /// PageModel for viewing user details. Admin-only access.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="DetailsModel"/> class.
        /// </summary>
        /// <param name="context">The application database context.</param>
        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets or sets the user whose details are being viewed.
        /// </summary>
        public ApplicationUser AppUser { get; set; } = new();

        /// <summary>
        /// Handles the GET request. Loads the user details.
        /// </summary>
        /// <param name="id">The ID of the user.</param>
        public async Task<IActionResult> OnGetAsync(string? id)
        {
            if (id == null) return NotFound();

            AppUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (AppUser == null) return NotFound();

            return Page();
        }
    }
}
