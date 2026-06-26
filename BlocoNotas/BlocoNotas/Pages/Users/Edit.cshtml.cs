using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BlocoNotas.Models;

namespace BlocoNotas.Pages.Users
{
    /// <summary>
    /// PageModel para editar o perfil de um utilizador. Acesso apenas para administradores.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        /// <summary>
        /// Construtor para injeção de dependências.
        /// </summary>
        /// <param name="userManager">Gerenciador de utilizadores.</param>
        public EditModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        /// <summary>
        /// Modelo de entrada para o formulário de edição.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; } = new();

        /// <summary>
        /// Modelo de entrada para o formulário de edição de utilizador.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            /// ID do utilizador.
            /// </summary>
            public string Id { get; set; } = string.Empty;

            /// <summary>
            /// Nome de utilizador.
            /// </summary>
            public string UserName { get; set; } = string.Empty;

            /// <summary>
            /// Endereço de email.
            /// </summary>
            public string Email { get; set; } = string.Empty;

            /// <summary>
            /// Nova password opcional.
            /// </summary>
            public string? Password { get; set; }
        }

        /// <summary>
        /// Endpoint GET. Carrega os dados do utilizador no formulário de edição.
        /// </summary>
        /// <param name="id">ID do utilizador a editar.</param>
        public async Task<IActionResult> OnGetAsync(string? id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            Input = new InputModel
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty
            };

            return Page();
        }

        /// <summary>
        /// Endpoint POST. Atualiza o perfil do utilizador e opcionalmente redefine a password.
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var user = await _userManager.FindByIdAsync(Input.Id);
            if (user == null) return NotFound();

            user.UserName = Input.UserName;
            user.Email = Input.Email;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
                return Page();
            }

            if (!string.IsNullOrWhiteSpace(Input.Password))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                result = await _userManager.ResetPasswordAsync(user, token, Input.Password);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                        ModelState.AddModelError("", error.Description);
                    return Page();
                }
            }

            return RedirectToPage("./Index");
        }
    }
}
