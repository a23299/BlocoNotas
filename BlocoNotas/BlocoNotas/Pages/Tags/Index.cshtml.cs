using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BlocoNotas.Data;
using BlocoNotas.Models;

namespace BlocoNotas.Pages.Tags
{
    /// <summary>
    /// PageModel para listar, criar e eliminar tags.
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
        /// Lista de tags ordenadas por nome.
        /// </summary>
        public IList<Tag> Tags { get; set; } = new List<Tag>();

        /// <summary>
        /// Endpoint GET. Obtém todas as tags.
        /// </summary>
        public async Task OnGetAsync()
        {
            Tags = await _context.Tags.OrderBy(t => t.Name).ToListAsync();
        }

        /// <summary>
        /// Endpoint POST de eliminação. Remove uma tag e as suas associações.
        /// </summary>
        /// <param name="id">ID da tag a eliminar.</param>
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
