using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BlocoNotas.Data;
using BlocoNotas.Models;

namespace BlocoNotas.Pages.NoteShares
{
    /// <summary>
    /// PageModel for editing a note share's permissions.
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
        /// Gets or sets the note share being edited.
        /// </summary>
        [BindProperty]
        public NoteShare NoteShare { get; set; } = new();

        /// <summary>
        /// Gets or sets the note title for display.
        /// </summary>
        public string? NoteTitle { get; set; }

        /// <summary>
        /// Gets or sets the shared-with user name for display.
        /// </summary>
        public string? SharedWithUserName { get; set; }

        /// <summary>
        /// Handles the GET request. Loads the share data for editing.
        /// </summary>
        /// <param name="id">The ID of the share to edit.</param>
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

        /// <summary>
        /// Handles the POST request. Updates the share's CanEdit permission.
        /// </summary>
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
