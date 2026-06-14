using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BlocoNotas.Data;
using BlocoNotas.Models;

namespace BlocoNotas.Pages.Tags
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Tag> Tags { get; set; } = new List<Tag>();

        public async Task OnGetAsync()
        {
            Tags = await _context.Tags.OrderBy(t => t.Name).ToListAsync();
        }

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
