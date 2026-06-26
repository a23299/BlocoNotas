using System.ComponentModel.DataAnnotations;
using BlocoNotas.ApiEmail.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BlocoNotas.Models;

namespace BlocoNotas.Areas.Identity.Pages.Account
{
    /// <summary>
    /// PageModel para registo de novos utilizadores.
    /// </summary>
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly ISendEmail _sendEmail;

        /// <summary>
        /// Construtor para injeção de dependências.
        /// </summary>
        /// <param name="userManager">Gerenciador de utilizadores.</param>
        /// <param name="signInManager">Gerenciador de autenticação.</param>
        /// <param name="logger">Instância do logger.</param>
        /// <param name="sendEmail">Serviço de email para envio de mensagens de boas-vindas.</param>
        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            ISendEmail sendEmail)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _sendEmail = sendEmail;
        }

        /// <summary>
        /// Modelo de entrada do formulário de registo.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; } = new();

        /// <summary>
        /// URL de retorno após o registo.
        /// </summary>
        public string ReturnUrl { get; set; } = string.Empty;

        /// <summary>
        /// Lista de esquemas de autenticação externa.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; } = new List<AuthenticationScheme>();

        /// <summary>
        /// Modelo de entrada para o formulário de registo.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            /// Nome de utilizador.
            /// </summary>
            [Required(ErrorMessage = "O nome de utilizador é obrigatório.")]
            [Display(Name = "Nome de Utilizador")]
            public string UserName { get; set; }

            /// <summary>
            /// Endereço de email.
            /// </summary>
            [Required(ErrorMessage = "O email é obrigatório.")]
            [EmailAddress(ErrorMessage = "Email inválido.")]
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            /// Password do utilizador.
            /// </summary>
            [Required(ErrorMessage = "A password é obrigatória.")]
            [StringLength(100, ErrorMessage = "A {0} deve ter pelo menos {2} e no máximo {1} caracteres.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            /// Confirmação da password.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirmar Password")]
            [Compare("Password", ErrorMessage = "As passwords não coincidem.")]
            public string ConfirmPassword { get; set; }
        }

        /// <summary>
        /// Endpoint GET. Prepara a página de registo.
        /// </summary>
        /// <param name="returnUrl">URL opcional de retorno após o registo.</param>
        public async Task OnGetAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        /// <summary>
        /// Endpoint POST de registo. Cria o utilizador, envia email de boas-vindas e inicia sessão.
        /// </summary>
        /// <param name="returnUrl">URL opcional de retorno após o registo.</param>
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = Input.UserName,
                    Email = Input.Email
                };

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    await _userManager.AddToRoleAsync(user, "Utilizador");

                    try
                    {
                        await _sendEmail.SendEmailAsync(
                            user.Email,
                            "Conta criada com sucesso",
                            $"Olá {user.UserName}, a tua conta foi criada com sucesso no BlocoNotas."
                        );
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Erro ao enviar email de boas-vindas");
                    }

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return Page();
        }
    }
}
