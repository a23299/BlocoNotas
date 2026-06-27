using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using BlocoNotas.Data;
using BlocoNotas.Hubs;
using BlocoNotas.Models;

namespace BlocoNotas.Pages.Notes
{
    /// <summary>
    /// PageModel para eliminar (soft delete) uma nota.
    /// </summary>
    [Authorize]
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NoteHub> _hubContext;

        /// <summary>
        /// Construtor para injeção de dependências.
        /// </summary>
        /// <param name="context">Contexto da base de dados.</param>
        /// <param name="hubContext">Contexto do hub SignalR para notificações em tempo real.</param>
        public DeleteModel(ApplicationDbContext context, IHubContext<NoteHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        /// <summary>
        /// Nota a ser eliminada.
        /// </summary>
        [BindProperty]
        public Note Note { get; set; } = new();

        /// <summary>
        /// Endpoint GET. Obtém os dados da nota para confirmação de eliminação.
        /// </summary>
        /// <param name="id">ID da nota a eliminar.</param>
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Note = await _context.Notes
                .Include(n => n.User)
                .FirstOrDefaultAsync(n => n.NoteId == id && n.UserFK == userId && !n.IsDeleted);

            if (Note == null) return NotFound();

            return Page();
        }

        /// <summary>
        /// Endpoint POST. Elimina logicamente a nota marcando o campo IsDeleted.
        /// </summary>
        /// <param name="id">ID da nota a eliminar.</param>
        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var note = await _context.Notes
                .FirstOrDefaultAsync(n => n.NoteId == id && n.UserFK == userId && !n.IsDeleted);

            if (note == null) return NotFound();

            note.IsDeleted = true;
            note.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            await _hubContext.Clients.User(userId)
                .SendAsync("ReceiveNotification", $"Nota '{note.Title}' foi eliminada.");

            TempData["Notification"] = $"Nota '{note.Title}' foi eliminada.";

            return RedirectToPage("./Index");
        }
    }
}
