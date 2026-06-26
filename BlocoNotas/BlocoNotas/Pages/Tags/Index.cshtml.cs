using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BlocoNotas.Data;
using BlocoNotas.Models;

namespace BlocoNotas.Pages.Tags
{
    /// <summary>
    /// PageModel for listing, creating, and deleting tags.
    /// </summary>
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexModel"/> class.
        /// </summary>
        /// <param name="context">The application database context.</param>
        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets or sets the list of tags ordered by name.
        /// </summary>
        public IList<Tag> Tags { get; set; } = new List<Tag>();

        /// <summary>
        /// Handles the GET request. Loads all tags.
        /// </summary>
        public async Task OnGetAsync()
        {
            Tags = await _context.Tags.OrderBy(t => t.Name).ToListAsync();
        }

        /// <summary>
        /// Handles the POST delete request. Removes a tag and its associations.
        /// </summary>
        /// <param name="id">The ID of the tag to delete.</param>
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var tag = await _context.Tags.FindAsync(id);
            if (tag != null)
            {
                var noteTags = await _context.NoteTags.Where(nt => nt.TagFK == id).ToListAsync();
                _context.NoteTags.RemoveRange(noteTags);
                _context.Tags.Remove(tag);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }
    }
}
