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
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<NoteShare> SharedByMe { get; set; } = new List<NoteShare>();
        public IList<Note> SharedWithMe { get; set; } = new List<Note>();

        public async Task OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            SharedByMe = await _context.NoteShares
                .Where(ns => ns.Note.UserFK == userId && !ns.Note.IsDeleted)
                .Include(ns => ns.Note)
                .Include(ns => ns.SharedWithUser)
                .OrderByDescending(ns => ns.SharedAt)
                .ToListAsync();

            var sharedWithMeShares = await _context.NoteShares
                .Where(ns => ns.UserShareFK == userId && !ns.Note.IsDeleted)
                .Include(ns => ns.Note)
                    .ThenInclude(n => n.User)
                .OrderByDescending(ns => ns.SharedAt)
                .ToListAsync();

            SharedWithMe = sharedWithMeShares.Select(ns => ns.Note).DistinctBy(n => n.NoteId).ToList();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var share = await _context.NoteShares
                .Include(ns => ns.Note)
                .FirstOrDefaultAsync(ns => ns.NoteShareId == id &&
                    (ns.Note.UserFK == userId || ns.UserShareFK == userId));

            if (share != null)
            {
                _context.NoteShares.Remove(share);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }
    }
}
