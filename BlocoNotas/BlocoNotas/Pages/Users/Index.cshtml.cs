using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BlocoNotas.Data;
using BlocoNotas.Models;

namespace BlocoNotas.Pages.Users
{
    /// <summary>
    /// PageModel para listar, eliminar e promover utilizadores. Acesso apenas para administradores.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Construtor para injeção de dependências.
        /// </summary>
        /// <param name="userManager">Gerenciador de utilizadores.</param>
        /// <param name="context">Contexto da base de dados.</param>
        public IndexModel(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        /// <summary>
        /// Lista de utilizadores.
        /// </summary>
        public IList<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();

        /// <summary>
        /// Dicionário que mapeia IDs de utilizadores ao seu estado de administrador.
        /// </summary>
        public Dictionary<string, bool> IsAdmin { get; set; } = new();

        /// <summary>
        /// Endpoint GET. Obtém todos os utilizadores e o seu estado de administrador.
        /// </summary>
        public async Task OnGetAsync()
        {
            Users = await _context.Users.OrderBy(u => u.UserName).ToListAsync();

            foreach (var user in Users)
            {
                IsAdmin[user.Id] = await _userManager.IsInRoleAsync(user, "Admin");
            }
        }

        /// <summary>
        /// Endpoint POST de eliminação. Elimina um utilizador, impedindo auto-eliminação.
        /// </summary>
        /// <param name="id">ID do utilizador a eliminar.</param>
        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                var currentUserId = _userManager.GetUserId(User);
                if (user.Id == currentUserId)
                {
                    ModelState.AddModelError("", "Não pode eliminar a sua própria conta.");
                    return RedirectToPage();
                }

                await _userManager.DeleteAsync(user);
            }

            return RedirectToPage();
        }

        /// <summary>
        /// Endpoint POST para promover. Promove um utilizador ao cargo de administrador.
        /// </summary>
        /// <param name="id">ID do utilizador a promover.</param>
        public async Task<IActionResult> OnPostMakeAdminAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                await _userManager.AddToRoleAsync(user, "Admin");
            }

            return RedirectToPage();
        }
    }
}
