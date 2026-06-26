using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BlocoNotas.Models;

namespace BlocoNotas.Areas.Identity.Pages.Account
{
    /// <summary>
    /// PageModel para autenticação de utilizadores.
    /// </summary>
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;

        /// <summary>
        /// Construtor para injeção de dependências.
        /// </summary>
        /// <param name="signInManager">Gerenciador de autenticação.</param>
        /// <param name="logger">Instância do logger.</param>
        public LoginModel(SignInManager<ApplicationUser> signInManager, ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        /// <summary>
        /// Modelo de entrada do formulário de login.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        /// Lista de esquemas de autenticação externa.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; } = new List<AuthenticationScheme>();

        /// <summary>
        /// URL de retorno após o login.
        /// </summary>
        public string ReturnUrl { get; set; } = string.Empty;

        /// <summary>
        /// Mensagem de erro a exibir.
        /// </summary>
        [TempData]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Modelo de entrada para o formulário de login.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            /// Endereço de email.
            /// </summary>
            [Required(ErrorMessage = "O email é obrigatório.")]
            [EmailAddress(ErrorMessage = "Email inválido.")]
            public string Email { get; set; }

            /// <summary>
            /// Password do utilizador.
            /// </summary>
            [Required(ErrorMessage = "A password é obrigatória.")]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            /// <summary>
            /// Indica se a sessão deve ser persistida.
            /// </summary>
            [Display(Name = "Lembrar-me?")]
            public bool RememberMe { get; set; }
        }

        /// <summary>
        /// Endpoint GET. Prepara a página de login.
        /// </summary>
        /// <param name="returnUrl">URL opcional de retorno após o login.</param>
        public async Task OnGetAsync(string? returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        /// <summary>
        /// Endpoint POST de login. Autentica o utilizador e redireciona em caso de sucesso.
        /// </summary>
        /// <param name="returnUrl">URL opcional de retorno após o login.</param>
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    return LocalRedirect(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Credenciais inválidas. Verifique o email e a password.");
                    return Page();
                }
            }

            return Page();
        }
    }
}
