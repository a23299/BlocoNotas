using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using BlocoNotas.Data;
using BlocoNotas.Hubs;
using BlocoNotas.Models;

namespace BlocoNotas.Pages.NoteShares
{
    /// <summary>
    /// PageModel para partilhar uma nota com outro utilizador.
    /// </summary>
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NoteHub> _hubContext;

        /// <summary>
        /// Construtor para injeção de dependências.
        /// </summary>
        /// <param name="context">Contexto da base de dados.</param>
        /// <param name="hubContext">Contexto do hub SignalR para notificações em tempo real.</param>
        public CreateModel(ApplicationDbContext context, IHubContext<NoteHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        /// <summary>
        /// Partilha de nota a ser criada.
        /// </summary>
        [BindProperty]
        public NoteShare NoteShare { get; set; } = new();

        /// <summary>
        /// Lista de seleção das notas do utilizador.
        /// </summary>
        public SelectList? NoteList { get; set; }

        /// <summary>
        /// Lista de seleção dos utilizadores disponíveis.
        /// </summary>
        public SelectList? UserList { get; set; }

        /// <summary>
        /// Endpoint GET. Obtém as listas de notas e utilizadores.
        /// </summary>
        /// <param name="noteId">ID opcional da nota pré-selecionada.</param>
        public async Task OnGetAsync(int? noteId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var userNotes = await _context.Notes
                .Where(n => n.UserFK == userId && !n.IsDeleted)
                .ToListAsync();

            NoteList = new SelectList(userNotes, "NoteId", "Title", noteId);
            UserList = new SelectList(_context.Users.Where(u => u.Id != userId), "Id", "UserName");

            if (noteId.HasValue)
                NoteShare.NoteId = noteId.Value;
        }

        /// <summary>
        /// Endpoint POST. Cria a partilha e envia uma notificação SignalR.
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var note = await _context.Notes
                .FirstOrDefaultAsync(n => n.NoteId == NoteShare.NoteId && n.UserFK == userId && !n.IsDeleted);

            if (note == null)
            {
                ModelState.AddModelError("", "Nota não encontrada ou não pertence a si.");
                await SetupViewData(userId);
                return Page();
            }

            if (NoteShare.UserShareFK == userId)
            {
                ModelState.AddModelError("", "Não pode partilhar uma nota consigo mesmo.");
                await SetupViewData(userId);
                return Page();
            }

            var existingShare = await _context.NoteShares
                .AnyAsync(ns => ns.NoteId == NoteShare.NoteId && ns.UserShareFK == NoteShare.UserShareFK);

            if (existingShare)
            {
                ModelState.AddModelError("", "Esta nota já está partilhada com este utilizador.");
                await SetupViewData(userId);
                return Page();
            }

            if (!ModelState.IsValid)
            {
                await SetupViewData(userId);
                return Page();
            }

            NoteShare.SharedAt = DateTime.Now;
            _context.NoteShares.Add(NoteShare);
            await _context.SaveChangesAsync();

            var currentUserName = User.Identity?.Name ?? "Alguém";
            var message = $"Nota '{note.Title}' foi partilhada consigo por {currentUserName}.";
            await _hubContext.Clients.User(NoteShare.UserShareFK).SendAsync("ReceiveNotification", message);

            return RedirectToPage("./Index");
        }

        /// <summary>
        /// Recarrega os dados das listas de seleção de notas e utilizadores.
        /// </summary>
        /// <param name="userId">ID do utilizador atual.</param>
        private async Task SetupViewData(string userId)
        {
            var userNotes = await _context.Notes
                .Where(n => n.UserFK == userId && !n.IsDeleted)
                .ToListAsync();

            NoteList = new SelectList(userNotes, "NoteId", "Title", NoteShare.NoteId);
            UserList = new SelectList(_context.Users.Where(u => u.Id != userId), "Id", "UserName");
        }
    }
}
