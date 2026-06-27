using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BlocoNotas.Data;
using BlocoNotas.Models;

namespace BlocoNotas.Pages.Users
{
    /// <summary>
    /// PageModel para visualizar detalhes de um utilizador. Acesso apenas para administradores.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Construtor para injeção de dependências.
        /// </summary>
        /// <param name="context">Contexto da base de dados.</param>
        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Utilizador cujos detalhes estão a ser visualizados.
        /// </summary>
        public ApplicationUser AppUser { get; set; } = new();

        /// <summary>
        /// Endpoint GET. Obtém os detalhes do utilizador.
        /// </summary>
        /// <param name="id">ID do utilizador.</param>
        public async Task<IActionResult> OnGetAsync(string? id)
        {
            if (id == null) return NotFound();

            AppUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (AppUser == null) return NotFound();

            return Page();
        }
    }
}
