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
    /// PageModel para listar todas as notas do utilizador autenticado.
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
        /// Lista de notas do utilizador atual.
        /// </summary>
        public IList<Note> Notes { get; set; } = new List<Note>();

        /// <summary>
        /// Endpoint GET. Obtém todas as notas não eliminadas do utilizador atual.
        /// </summary>
        public async Task OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Notes = await _context.Notes
                .Include(n => n.NoteTags)
                    .ThenInclude(nt => nt.Tag)
                .Include(n => n.User)
                .Where(n => n.UserFK == userId && !n.IsDeleted)
                .OrderByDescending(n => n.UpdatedAt)
                .ToListAsync();
        }
    }
}
