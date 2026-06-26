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
        /// Lista de todas as tags disponíveis para filtro e gestão.
        /// </summary>
        public IList<Tag> AvailableTags { get; set; } = new List<Tag>();

        /// <summary>
        /// ID da tag selecionada para filtro.
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public int? SelectedTagId { get; set; }

        /// <summary>
        /// Endpoint GET. Obtém as notas do utilizador, opcionalmente filtradas por tag.
        /// </summary>
        public async Task OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            AvailableTags = await _context.Tags.OrderBy(t => t.Name).ToListAsync();

            var query = _context.Notes
                .Include(n => n.NoteTags)
                    .ThenInclude(nt => nt.Tag)
                .Include(n => n.User)
                .Where(n => n.UserFK == userId && !n.IsDeleted);

            if (SelectedTagId.HasValue)
            {
                query = query.Where(n => n.NoteTags.Any(nt => nt.TagFK == SelectedTagId.Value));
            }

            Notes = await query.OrderByDescending(n => n.UpdatedAt).ToListAsync();
        }

        /// <summary>
        /// Endpoint POST para atualizar as tags associadas a uma nota.
        /// </summary>
        /// <param name="noteId">ID da nota.</param>
        /// <param name="tagIds">Lista de IDs das tags a associar.</param>
        public async Task<IActionResult> OnPostUpdateTagsAsync(int noteId, List<int> tagIds)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var note = await _context.Notes
                .Include(n => n.NoteTags)
                .FirstOrDefaultAsync(n => n.NoteId == noteId && n.UserFK == userId && !n.IsDeleted);

            if (note == null) return NotFound();

            tagIds ??= new List<int>();

            var existingTagIds = note.NoteTags.Select(nt => nt.TagFK).ToList();
            var toRemove = note.NoteTags.Where(nt => !tagIds.Contains(nt.TagFK)).ToList();
            var toAdd = tagIds.Where(tid => !existingTagIds.Contains(tid)).ToList();

            _context.NoteTags.RemoveRange(toRemove);

            foreach (var tagId in toAdd)
            {
                _context.NoteTags.Add(new NoteTag { NoteTagFK = noteId, TagFK = tagId });
            }

            await _context.SaveChangesAsync();

            return RedirectToPage(new { SelectedTagId });
        }
    }
}
