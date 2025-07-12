using BlocoNotas.ApiEmail.Services;
using BlocoNotas.Data;
using BlocoNotas.Models;
using BlocoNotas.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BlocoNotas.Controllers.Api
{
    /// <summary>
    /// Controlador responsável por autenticação e registo de utilizadores.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly TokenService _tokenService;
        private readonly ISendEmail _sendEmail;

        /// <summary>
        /// Construtor para injeção de dependências no controlador Auth.
        /// </summary>
        /// <param name="userManager">Gerenciador de utilizadores.</param>
        /// <param name="signInManager">Gerenciador de autenticação.</param>
        /// <param name="context">Contexto da base de dados.</param>
        /// <param name="tokenService">Serviço para geração de tokens JWT.</param>
        /// <param name="sendEmail">Serviço para envio de emails.</param>
        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context,
            TokenService tokenService,
            ISendEmail sendEmail)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _tokenService = tokenService;
            _sendEmail = sendEmail;
        }

        /// <summary>
        /// Endpoint para autenticar um utilizador com email e password.
        /// </summary>
        /// <param name="request">Objeto contendo email e password.</param>
        /// <returns>Token JWT e dados do utilizador autenticado.</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return Unauthorized(new { message = "Email não encontrado" });

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!result.Succeeded)
                return Unauthorized(new { message = "Credenciais inválidas" });

            var token = await _tokenService.GenerateToken(user);

            var roles = await _userManager.GetRolesAsync(user); // obter roles

            return Ok(new
            {
                token,
                user = new
                {
                    id = user.Id,
                    username = user.UserName,
                    email = user.Email,
                    roles = roles // adiciona roles aqui
                }
            });
        }
        
        /// <summary>
        /// Endpoint para registar um novo utilizador.
        /// </summary>
        /// <param name="request">Objeto contendo username, email e password para registo.</param>
        /// <returns>Token JWT e dados do utilizador criado.</returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var userExists = await _userManager.FindByNameAsync(request.Username);
            if (userExists != null)
                return BadRequest(new { message = "Nome de utilizador já existe" });

            var user = new ApplicationUser
            {
                UserName = request.Username,
                Email = request.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
            
            // Adiciona role padrão
            await _userManager.AddToRoleAsync(user, "Utilizador");
            
            // Gera o token JWT
            var token = await _tokenService.GenerateToken(user);
            
            // Enviar email de confirmação
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
                Console.WriteLine("Erro ao enviar email: " + ex.Message);
            }

            // Return do Token e Dados do Utilizador
            return Ok(new
            {
                message = "Registo efetuado com sucesso.",
                token,
                user = new
                {
                    id = user.Id,
                    username = user.UserName,
                    email = user.Email
                }
            });
        }
    }

    /// <summary>
    /// Modelo para requisição de login.
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// Email do utilizador.
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// Password do utilizador.
        /// </summary>
        public string Password { get; set; }
    }

    /// <summary>
    /// Modelo para requisição de registo.
    /// </summary>
    public class RegisterRequest
    {
        /// <summary>
        /// Nome do utilizador.
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// Email do utilizador.
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// Password do utilizador.
        /// </summary>
        public string Password { get; set; }
    }
}
