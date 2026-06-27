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
    /// PageModel para visualizar partilhas de notas (partilhadas por mim e comigo).
    /// </summary>
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Construtor para injeção de dependências.
        /// </summary>
        /// <param name="context">Contexto da base de dados.</param>
        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lista de notas partilhadas pelo utilizador atual.
        /// </summary>
        public IList<NoteShare> SharedByMe { get; set; } = new List<NoteShare>();

        /// <summary>
        /// Lista de notas partilhadas com o utilizador atual.
        /// </summary>
        public IList<Note> SharedWithMe { get; set; } = new List<Note>();

        /// <summary>
        /// Endpoint GET. Obtém as partilhas feitas e recebidas pelo utilizador atual.
        /// </summary>
        public async Task OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            SharedByMe = await _context.NoteShares
                .Where(ns => ns.Note!.UserFK == userId && !ns.Note!.IsDeleted)
                .Include(ns => ns.Note)
                .Include(ns => ns.SharedWithUser)
                .OrderByDescending(ns => ns.SharedAt)
                .ToListAsync();

            var sharedWithMeShares = await _context.NoteShares
                .Where(ns => ns.UserShareFK == userId && !ns.Note!.IsDeleted)
                .Include(ns => ns.Note)
                    .ThenInclude(n => n.User)
                .OrderByDescending(ns => ns.SharedAt)
                .ToListAsync();

            SharedWithMe = sharedWithMeShares.Select(ns => ns.Note).Where(n => n != null).Select(n => n!).DistinctBy(n => n.NoteId).ToList();
        }

        /// <summary>
        /// Endpoint POST de eliminação. Remove uma partilha se o utilizador for o proprietário ou o destinatário.
        /// </summary>
        /// <param name="id">ID da partilha a eliminar.</param>
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
