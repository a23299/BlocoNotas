using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BlocoNotas.Data;
using BlocoNotas.Models;
using BlocoNotas.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlocoNotas.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
public class AuthController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly TokenService _tokenService;

    public AuthController(ApplicationDbContext context, TokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.UserName == request.Username);

        if (user == null)
            return Unauthorized(new { message = "User não encontrado" });

        // Aqui usamos uma verificação simples, mas você pode melhorar isso
        // usando um algoritmo de hash como BCrypt
        if (user.Password != HashPassword(request.Password))
            return Unauthorized(new { message = "Senha incorreta" });

        var token = _tokenService.GenerateToken(user);

        return Ok(new
        {
            token,
            user = new
            {
                id = user.UserId,
                username = user.UserName
            }
        });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.UserName == request.Username))
            return BadRequest(new { message = "Nome de user já existe" });

        var user = new User
        {
            UserName = request.Username,
            Password = HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = _tokenService.GenerateToken(user);

        return Ok(new
        {
            token,
            user = new
            {
                id = user.UserId,
                username = user.UserName
            }
        });
    }

    // Método simples para hash de senha
    // Em produção, considere usar BCrypt ou outro algoritmo mais seguro
    private string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
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