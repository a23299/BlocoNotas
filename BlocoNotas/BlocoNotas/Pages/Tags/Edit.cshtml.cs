using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BlocoNotas.Data;
using BlocoNotas.Models;

namespace BlocoNotas.Pages.Tags
{
    /// <summary>
    /// PageModel for editing an existing tag.
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
        /// Gets or sets the tag being edited.
        /// </summary>
        [BindProperty]
        public Tag Tag { get; set; } = new();

        /// <summary>
        /// Handles the GET request. Loads the tag for editing.
        /// </summary>
        /// <param name="id">The ID of the tag to edit.</param>
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            Tag = await _context.Tags.FindAsync(id);
            if (Tag == null) return NotFound();

            return Page();
        }

        /// <summary>
        /// Handles the POST request. Updates the tag after validating for duplicates.
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var existing = await _context.Tags
                .AnyAsync(t => t.Name == Tag.Name && t.TagId != Tag.TagId);
            if (existing)
            {
                ModelState.AddModelError("Tag.Name", "Já existe uma tag com esse nome.");
                return Page();
            }

            _context.Attach(Tag).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
