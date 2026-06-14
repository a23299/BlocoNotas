using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BlocoNotas.Data;
using BlocoNotas.Models;

namespace BlocoNotas.Pages.NoteShares
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public NoteShare NoteShare { get; set; } = new();

        public string? NoteTitle { get; set; }
        public string? SharedWithUserName { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            NoteShare = await _context.NoteShares
                .Include(ns => ns.Note)
                .Include(ns => ns.SharedWithUser)
                .FirstOrDefaultAsync(ns => ns.NoteShareId == id && ns.Note.UserFK == userId);

            if (NoteShare == null) return NotFound();

            NoteTitle = NoteShare.Note?.Title;
            SharedWithUserName = NoteShare.SharedWithUser?.UserName;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var existingShare = await _context.NoteShares
                .Include(ns => ns.Note)
                .FirstOrDefaultAsync(ns => ns.NoteShareId == NoteShare.NoteShareId && ns.Note.UserFK == userId);

            if (existingShare == null) return NotFound();

            existingShare.CanEdit = NoteShare.CanEdit;
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
