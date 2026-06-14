using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BlocoNotas.Data;
using BlocoNotas.Models;

namespace BlocoNotas.Pages.Users
{
    [Authorize(Roles = "Admin")]
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public ApplicationUser AppUser { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(string? id)
        {
            if (id == null) return NotFound();

            AppUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (AppUser == null) return NotFound();

            return Page();
        }
    }
}
