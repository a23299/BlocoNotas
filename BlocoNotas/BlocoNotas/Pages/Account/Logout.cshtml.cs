using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BlocoNotas.Models;

namespace BlocoNotas.Pages.Account
{
    /// <summary>
    /// PageModel para terminar a sessão do utilizador atual.
    /// </summary>
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LogoutModel> _logger;

        /// <summary>
        /// Construtor para injeção de dependências.
        /// </summary>
        /// <param name="signInManager">Gerenciador de autenticação.</param>
        /// <param name="logger">Instância do logger.</param>
        public LogoutModel(SignInManager<ApplicationUser> signInManager, ILogger<LogoutModel> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        /// <summary>
        /// Endpoint POST de logout. Termina a sessão do utilizador e redireciona para a página inicial.
        /// </summary>
        /// <param name="returnUrl">URL opcional para redirecionar após o logout.</param>
        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            returnUrl ??= Url.Page("/Index");
            return LocalRedirect(returnUrl);
        }
    }
}
