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
    /// PageModel para visualizar uma nota partilhada (apenas leitura).
    /// </summary>
    [Authorize]
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
        /// Nota partilhada a ser visualizada.
        /// </summary>
        public Note Note { get; set; } = new();

        /// <summary>
        /// Nome do utilizador que partilhou a nota.
        /// </summary>
        public string SharedByUserName { get; set; } = string.Empty;

        /// <summary>
        /// Indica se o utilizador atual pode editar a nota.
        /// </summary>
        public bool CanEdit { get; set; }

        /// <summary>
        /// Endpoint GET. Obtém os detalhes da nota partilhada.
        /// </summary>
        /// <param name="noteId">ID da nota partilhada.</param>
        public async Task<IActionResult> OnGetAsync(int? noteId)
        {
            if (noteId == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var share = await _context.NoteShares
                .Include(ns => ns.Note)
                    .ThenInclude(n => n.User)
                .FirstOrDefaultAsync(ns => ns.NoteId == noteId &&
                    (ns.UserShareFK == userId || ns.Note.UserFK == userId) &&
                    !ns.Note.IsDeleted);

            if (share == null) return NotFound();

            Note = share.Note!;
            CanEdit = share.CanEdit;
            SharedByUserName = share.Note?.User?.UserName ?? "Desconhecido";

            return Page();
        }
    }
}
