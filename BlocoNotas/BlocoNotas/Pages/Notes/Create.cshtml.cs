using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BlocoNotas.Data;
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

        /// <summary>
        /// Construtor para injeção de dependências.
        /// </summary>
        /// <param name="context">Contexto da base de dados.</param>
        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Nota a ser criada.
        /// </summary>
        [BindProperty]
        public Note Note { get; set; } = new();

        /// <summary>
        /// Endpoint GET. Apresenta a página de criação.
        /// </summary>
        public IActionResult OnGet()
        {
            return Page();
        }

        /// <summary>
        /// Endpoint POST. Cria uma nova nota e redireciona para a página inicial.
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Note.UserFK = userId;
            Note.CreatedAt = DateTime.Now;
            Note.UpdatedAt = DateTime.Now;
            Note.IsDeleted = false;

            _context.Notes.Add(Note);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
