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
    /// PageModel para editar uma nota existente.
    /// </summary>
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NoteHub> _hubContext;

        /// <summary>
        /// Construtor para injeção de dependências.
        /// </summary>
        /// <param name="context">Contexto da base de dados.</param>
        /// <param name="hubContext">Contexto do hub SignalR para notificações em tempo real.</param>
        public EditModel(ApplicationDbContext context, IHubContext<NoteHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        /// <summary>
        /// Nota a ser editada.
        /// </summary>
        [BindProperty]
        public Note Note { get; set; } = new();

        /// <summary>
        /// Lista de tags disponíveis para exibição.
        /// </summary>
        public IList<Tag> AvailableTags { get; set; } = new List<Tag>();

        /// <summary>
        /// Lista de IDs das tags associadas à nota.
        /// </summary>
        [BindProperty]
        public List<int> SelectedTagIds { get; set; } = new();

        /// <summary>
        /// Endpoint GET. Obtém a nota e as tags disponíveis para edição.
        /// </summary>
        /// <param name="id">ID da nota a editar.</param>
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Note = await _context.Notes
                .Include(n => n.NoteTags)
                .FirstOrDefaultAsync(n => n.NoteId == id && !n.IsDeleted &&
                    (n.UserFK == userId || n.SharedWith.Any(s => s.UserShareFK == userId && s.CanEdit)));

            if (Note == null) return NotFound();

            AvailableTags = await _context.Tags.OrderBy(t => t.Name).ToListAsync();
            SelectedTagIds = Note.NoteTags.Select(nt => nt.TagFK).ToList();

            return Page();
        }

        /// <summary>
        /// Endpoint POST. Atualiza a nota e as tags associadas, e redireciona para a página inicial.
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                AvailableTags = await _context.Tags.OrderBy(t => t.Name).ToListAsync();
                return Page();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var existingNote = await _context.Notes
                .Include(n => n.NoteTags)
                .FirstOrDefaultAsync(n => n.NoteId == Note.NoteId && !n.IsDeleted &&
                    (n.UserFK == userId || n.SharedWith.Any(s => s.UserShareFK == userId && s.CanEdit)));

            if (existingNote == null) return NotFound();

            existingNote.Title = Note.Title;
            existingNote.Content = Note.Content;
            existingNote.UpdatedAt = DateTime.Now;

            SelectedTagIds ??= new List<int>();
            var existingTagIds = existingNote.NoteTags.Select(nt => nt.TagFK).ToList();
            var toRemove = existingNote.NoteTags.Where(nt => !SelectedTagIds.Contains(nt.TagFK)).ToList();
            var toAdd = SelectedTagIds.Where(tid => !existingTagIds.Contains(tid)).ToList();

            _context.NoteTags.RemoveRange(toRemove);
            foreach (var tagId in toAdd)
            {
                _context.NoteTags.Add(new NoteTag { NoteTagFK = Note.NoteId, TagFK = tagId });
            }

            await _context.SaveChangesAsync();

            await _hubContext.Clients.User(userId)
                .SendAsync("ReceiveNotification", $"Nota '{existingNote.Title}' foi atualizada.");

            TempData["Notification"] = $"Nota '{existingNote.Title}' foi atualizada.";

            return RedirectToPage("./Index");
        }

        /// <summary>
        /// Endpoint POST JSON para criar uma nova tag sem perder o estado do formulário.
        /// </summary>
        /// <param name="name">Nome da nova tag.</param>
        /// <param name="color">Cor hexadecimal da nova tag.</param>
        public async Task<JsonResult> OnPostCreateTagJsonAsync(string name, string color)
        {
            if (string.IsNullOrWhiteSpace(name))
                return new JsonResult(new { error = "Nome obrigatório." }) { StatusCode = 400 };

            var exists = await _context.Tags.AnyAsync(t => t.Name == name);
            if (exists)
            {
                var existing = await _context.Tags.FirstAsync(t => t.Name == name);
                return new JsonResult(new { id = existing.TagId, name = existing.Name, color = existing.Color });
            }

            var tag = new Tag { Name = name, Color = color ?? "#0d6efd" };
            _context.Tags.Add(tag);
            await _context.SaveChangesAsync();

            return new JsonResult(new { id = tag.TagId, name = tag.Name, color = tag.Color });
        }
    }
}
