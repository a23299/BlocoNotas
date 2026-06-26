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
    /// PageModel para editar uma nota existente.
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
        public IList<int> SelectedTagIds { get; set; } = new List<int>();

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
                .FirstOrDefaultAsync(n => n.NoteId == id && n.UserFK == userId && !n.IsDeleted);

            if (Note == null) return NotFound();

            AvailableTags = await _context.Tags.ToListAsync();
            SelectedTagIds = Note.NoteTags.Select(nt => nt.TagFK).ToList();

            return Page();
        }

        /// <summary>
        /// Endpoint POST. Atualiza a nota e redireciona para a página inicial.
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var existingNote = await _context.Notes
                .Include(n => n.NoteTags)
                .FirstOrDefaultAsync(n => n.NoteId == Note.NoteId && n.UserFK == userId && !n.IsDeleted);

            if (existingNote == null) return NotFound();

            existingNote.Title = Note.Title;
            existingNote.Content = Note.Content;
            existingNote.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
