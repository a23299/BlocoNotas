using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BlocoNotas.Data;
using BlocoNotas.Models;

namespace BlocoNotas.Pages.Notes
{
    /// <summary>
    /// PageModel for editing an existing note.
    /// </summary>
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="EditModel"/> class.
        /// </summary>
        /// <param name="context">The application database context.</param>
        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets or sets the note being edited.
        /// </summary>
        [BindProperty]
        public Note Note { get; set; } = new();

        /// <summary>
        /// Gets or sets the list of available tags for display.
        /// </summary>
        public IList<Tag> AvailableTags { get; set; } = new List<Tag>();

        /// <summary>
        /// Gets or sets the list of tag IDs currently associated with the note.
        /// </summary>
        public IList<int> SelectedTagIds { get; set; } = new List<int>();

        /// <summary>
        /// Handles the GET request. Loads the note and available tags for editing.
        /// </summary>
        /// <param name="id">The ID of the note to edit.</param>
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

        /// <summary>
        /// Handles the POST request. Updates the note and redirects to the index page.
        /// </summary>
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
