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
    /// PageModel for user registration.
    /// </summary>
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly ISendEmail _sendEmail;

        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterModel"/> class.
        /// </summary>
        /// <param name="userManager">The Identity user manager.</param>
        /// <param name="signInManager">The Identity sign-in manager.</param>
        /// <param name="logger">The logger instance.</param>
        /// <param name="sendEmail">The email service for sending welcome emails.</param>
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
        /// Gets or sets the registration input model.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; } = new();

        /// <summary>
        /// Gets or sets the return URL after registration.
        /// </summary>
        public string ReturnUrl { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the list of external authentication schemes.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; } = new List<AuthenticationScheme>();

        /// <summary>
        /// Input model for the registration form.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            /// Gets or sets the username.
            /// </summary>
            [Required(ErrorMessage = "O nome de utilizador é obrigatório.")]
            [Display(Name = "Nome de Utilizador")]
            public string UserName { get; set; }

            /// <summary>
            /// Gets or sets the email address.
            /// </summary>
            [Required(ErrorMessage = "O email é obrigatório.")]
            [EmailAddress(ErrorMessage = "Email inválido.")]
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            /// Gets or sets the password.
            /// </summary>
            [Required(ErrorMessage = "A password é obrigatória.")]
            [StringLength(100, ErrorMessage = "A {0} deve ter pelo menos {2} e no máximo {1} caracteres.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            /// Gets or sets the password confirmation.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirmar Password")]
            [Compare("Password", ErrorMessage = "As passwords não coincidem.")]
            public string ConfirmPassword { get; set; }
        }

        /// <summary>
        /// Handles the GET request. Prepares the registration page.
        /// </summary>
        /// <param name="returnUrl">Optional return URL after registration.</param>
        public async Task OnGetAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        /// <summary>
        /// Handles the POST registration request. Creates the user, sends a welcome email, and signs in.
        /// </summary>
        /// <param name="returnUrl">Optional return URL after registration.</param>
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
