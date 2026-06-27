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
    /// PageModel para editar as permissões de uma partilha de nota.
    /// </summary>
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Construtor para injeção de dependências.
        /// </summary>
        /// <param name="context">Contexto da base de dados.</param>
        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Partilha de nota a ser editada.
        /// </summary>
        [BindProperty]
        public NoteShare NoteShare { get; set; } = new();

        /// <summary>
        /// Título da nota para exibição.
        /// </summary>
        public string? NoteTitle { get; set; }

        /// <summary>
        /// Nome do utilizador com quem foi partilhado para exibição.
        /// </summary>
        public string? SharedWithUserName { get; set; }

        /// <summary>
        /// Endpoint GET. Obtém os dados da partilha para edição.
        /// </summary>
        /// <param name="id">ID da partilha a editar.</param>
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
        /// Endpoint POST. Atualiza a permissão de edição da partilha.
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
