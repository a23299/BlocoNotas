using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BlocoNotas.Data;
using BlocoNotas.Models;

namespace BlocoNotas.Pages.Tags
{
    /// <summary>
    /// PageModel para editar uma tag existente.
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
        /// Tag a ser editada.
        /// </summary>
        [BindProperty]
        public Tag Tag { get; set; } = new();

        /// <summary>
        /// Endpoint GET. Obtém a tag para edição.
        /// </summary>
        /// <param name="id">ID da tag a editar.</param>
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            Tag = await _context.Tags.FindAsync(id);
            if (Tag == null) return NotFound();

            return Page();
        }

        /// <summary>
        /// Endpoint POST. Atualiza a tag após validação de duplicados.
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var existing = await _context.Tags
                .AnyAsync(t => t.Name == Tag.Name && t.TagId != Tag.TagId);
            if (existing)
            {
                ModelState.AddModelError("Tag.Name", "Já existe uma tag com esse nome.");
                return Page();
            }

            _context.Attach(Tag).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
