using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using BlocoNotas.Models; // para os ViewModels

/// <summary>
/// Controller responsável pelas operações de autenticação e registro de usuários.
/// </summary>
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    /// <summary>
    /// Construtor que injeta o UserManager e SignInManager para gerenciamento de usuários e autenticação.
    /// </summary>
    /// <param name="userManager">Gerenciador de usuários do Identity.</param>
    /// <param name="signInManager">Gerenciador de autenticação do Identity.</param>
    public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    /// <summary>
    /// Exibe a página de registro.
    /// </summary>
    /// <returns>View do formulário de registro.</returns>
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    /// <summary>
    /// Processa o registro do usuário.
    /// </summary>
    /// <param name="model">Dados do formulário de registro.</param>
    /// <returns>Redireciona para Home em caso de sucesso, ou retorna a view com erros.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(Register model)
    {
        if (ModelState.IsValid)
        {
            var user = new ApplicationUser { UserName = model.UserName, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Adiciona o usuário ao papel padrão "Utilizador"
                await _userManager.AddToRoleAsync(user, "Utilizador");
                
                // Realiza login automático após o registro
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            // Adiciona erros ao ModelState para exibição na view
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }
        // Caso haja erro, retorna a view com os dados preenchidos e erros
        return View(model);
    }

    /// <summary>
    /// Exibe a página de login.
    /// </summary>
    /// <returns>View do formulário de login.</returns>
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    /// <summary>
    /// Processa o login do usuário.
    /// </summary>
    /// <param name="model">Dados do formulário de login.</param>
    /// <returns>Redireciona para Home em caso de sucesso, ou retorna a view com erro.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(Login model)
    {
        if (ModelState.IsValid)
        {
            // Realiza tentativa de login com email e senha
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            // Caso falhe, adiciona erro genérico ao ModelState
            ModelState.AddModelError("", "Invalid login attempt.");
        }
        return View(model);
    }

    /// <summary>
    /// Realiza logout do usuário autenticado.
    /// </summary>
    /// <returns>Redireciona para a página inicial após logout.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}
