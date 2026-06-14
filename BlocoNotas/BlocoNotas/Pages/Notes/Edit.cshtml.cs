using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BlocoNotas.Data;
using BlocoNotas.Models;

namespace BlocoNotas.Pages.Notes
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
        public Note Note { get; set; } = new();

        public IList<Tag> AvailableTags { get; set; } = new List<Tag>();
        public IList<int> SelectedTagIds { get; set; } = new List<int>();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Note = await _context.Notes
                .Include(n => n.NoteTags)
                .FirstOrDefaultAsync(n => n.NoteId == id && n.UserFK == userId && !n.IsDeleted);

            if (Note == null) return NotFound();

            AvailableTags = await _context.Tags.ToListAsync();
            SelectedTagIds = Note.NoteTags.Select(nt => nt.TagFK).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var existingNote = await _context.Notes
                .Include(n => n.NoteTags)
                .FirstOrDefaultAsync(n => n.NoteId == Note.NoteId && n.UserFK == userId && !n.IsDeleted);

            if (existingNote == null) return NotFound();

            existingNote.Title = Note.Title;
            existingNote.Content = Note.Content;
            existingNote.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
