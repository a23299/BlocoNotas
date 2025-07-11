using System.Linq;
using System.Threading.Tasks;
using BlocoNotas.ApiEmail.Services;
using BlocoNotas.Data;
using BlocoNotas.Models;
using BlocoNotas.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using BlocoNotas.Models; // ApplicationUser
using BlocoNotas.Services; // TokenService, que gera o JWT

namespace BlocoNotas.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ApplicationDbContext _context;
    private readonly TokenService _tokenService;
    private readonly ISendEmail _sendEmail;

    //Email
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

public class LoginRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}

public class RegisterRequest
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}

