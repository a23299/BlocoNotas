using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BlocoNotas.Data;
using BlocoNotas.Models;

namespace BlocoNotas.Pages.Tags
{
    /// <summary>
    /// PageModel para criar uma nova tag.
    /// </summary>
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Construtor para injeção de dependências.
        /// </summary>
        /// <param name="context">Contexto da base de dados.</param>
        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tag a ser criada.
        /// </summary>
        [BindProperty]
        public Tag Tag { get; set; } = new();

        /// <summary>
        /// Endpoint GET. Apresenta a página de criação.
        /// </summary>
        public IActionResult OnGet()
        {
            return Page();
        }

        /// <summary>
        /// Endpoint POST. Cria a tag após validação de duplicados.
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var exists = _context.Tags.Any(t => t.Name == Tag.Name);
            if (exists)
            {
                ModelState.AddModelError("Tag.Name", "Já existe uma tag com esse nome.");
                return Page();
            }

            _context.Tags.Add(Tag);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
