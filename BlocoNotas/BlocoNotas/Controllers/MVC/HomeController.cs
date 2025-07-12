using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BlocoNotas.Models;

namespace BlocoNotas.Controllers.MVC
{
    /// <summary>
    /// Controller responsável pelas páginas principais do site, como home, login, registro e erro.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        /// <summary>
        /// Construtor que injeta o logger para registrar eventos e erros.
        /// </summary>
        /// <param name="logger">Instância de logger para a classe HomeController.</param>
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Exibe a página inicial do site.
        /// </summary>
        /// <returns>View da página inicial.</returns>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Exibe a página de informações do sistema ou do site.
        /// </summary>
        /// <returns>View da página de informações.</returns>
        public IActionResult Info()
        {
            return View();
        }

        /// <summary>
        /// Exibe a página de login.
        /// </summary>
        /// <returns>View do formulário de login.</returns>
        public IActionResult Login()
        {
            return View();
        }

        /// <summary>
        /// Exibe a página de erro.
        /// A resposta é configurada para não ser armazenada em cache.
        /// </summary>
        /// <returns>View da página de erro com detalhes do RequestId.</returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel 
            { 
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier 
            });
        }

        /// <summary>
        /// Exibe a página de registro de novos usuários.
        /// </summary>
        /// <returns>View do formulário de registro.</returns>
        public IActionResult Register()
        {
            return View();
        }
    }
}
