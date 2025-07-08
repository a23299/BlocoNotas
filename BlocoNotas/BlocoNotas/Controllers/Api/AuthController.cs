using System.Linq;
using System.Threading.Tasks;
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

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ApplicationDbContext context,
        TokenService tokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByNameAsync(request.Username);
        if (user == null)
            return Unauthorized(new { message = "Utilizador não encontrado" });

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!result.Succeeded)
            return Unauthorized(new { message = "Credenciais inválidas" });

        var token = _tokenService.GenerateToken(user);

        return Ok(new
        {
            token,
            user = new
            {
                id = user.Id,
                username = user.UserName
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
            Email = request.Username // opcional
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        var token = _tokenService.GenerateToken(user);

        return Ok(new
        {
            token,
            user = new
            {
                id = user.Id,
                username = user.UserName
            }
        });
    }
}

public class LoginRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
}

public class RegisterRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
}
