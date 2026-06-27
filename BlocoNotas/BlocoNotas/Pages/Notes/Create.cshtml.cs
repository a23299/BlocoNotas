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
    /// PageModel para criar uma nova nota.
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
        /// Nota a ser criada.
        /// </summary>
        [BindProperty]
        public Note Note { get; set; } = new();

        /// <summary>
        /// Lista de todas as tags disponíveis para seleção.
        /// </summary>
        public IList<Tag> AvailableTags { get; set; } = new List<Tag>();

        /// <summary>
        /// IDs das tags selecionadas para associar à nota.
        /// </summary>
        [BindProperty]
        public List<int> SelectedTagIds { get; set; } = new();

        /// <summary>
        /// Endpoint GET. Apresenta a página de criação.
        /// </summary>
        public async Task<IActionResult> OnGet()
        {
            AvailableTags = await _context.Tags.OrderBy(t => t.Name).ToListAsync();
            return Page();
        }

        /// <summary>
        /// Endpoint POST. Cria uma nova nota e as associações de tags, e redireciona para a página inicial.
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                AvailableTags = await _context.Tags.OrderBy(t => t.Name).ToListAsync();
                return Page();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Note.UserFK = userId;
            Note.CreatedAt = DateTime.Now;
            Note.UpdatedAt = DateTime.Now;
            Note.IsDeleted = false;

            _context.Notes.Add(Note);
            await _context.SaveChangesAsync();

            foreach (var tagId in SelectedTagIds)
            {
                _context.NoteTags.Add(new NoteTag { NoteTagFK = Note.NoteId, TagFK = tagId });
            }
            await _context.SaveChangesAsync();

            await _hubContext.Clients.User(userId)
                .SendAsync("ReceiveNotification", $"Nota '{Note.Title}' foi criada.");

            TempData["Notification"] = $"Nota '{Note.Title}' foi criada.";

            return RedirectToPage("./Index");
        }

        /// <summary>
        /// Endpoint POST JSON para criar uma nova tag sem perder o estado do formulário.
        /// </summary>
        /// <param name="name">Nome da nova tag.</param>
        public async Task<JsonResult> OnPostCreateTagJsonAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return new JsonResult(new { error = "Nome obrigatório." }) { StatusCode = 400 };

            var exists = await _context.Tags.AnyAsync(t => t.Name == name);
            if (exists)
            {
                var existing = await _context.Tags.FirstAsync(t => t.Name == name);
                return new JsonResult(new { id = existing.TagId, name = existing.Name });
            }

            var tag = new Tag { Name = name };
            _context.Tags.Add(tag);
            await _context.SaveChangesAsync();

            return new JsonResult(new { id = tag.TagId, name = tag.Name });
        }
    }
}
