using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BlocoNotas.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace BlocoNotas.Services
{
    /// <summary>
    /// Serviço responsável por gerar tokens JWT para autenticação.
    /// </summary>
    public class TokenService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;

        /// <summary>
        /// Inicializa uma nova instância do serviço de token.
        /// </summary>
        /// <param name="configuration">Configurações da aplicação para obter parâmetros JWT.</param>
        /// <param name="userManager">Gerenciador de usuários do Identity.</param>
        public TokenService(IConfiguration configuration, UserManager<ApplicationUser> userManager)
        {
            _configuration = configuration;
            _userManager = userManager;
        }

        /// <summary>
        /// Gera um token JWT para o utilizador especificado.
        /// </summary>
        /// <param name="applicationUser">O utilizador para quem o token será gerado.</param>
        /// <returns>Uma string contendo o token JWT.</returns>
        public async Task<string> GenerateToken(ApplicationUser applicationUser)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

            // Buscar roles do user
            var roles = await _userManager.GetRolesAsync(applicationUser);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, applicationUser.Id.ToString()),
                new Claim(ClaimTypes.Name, applicationUser.UserName)
            };

            // Adicionar as roles como claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(double.Parse(_configuration["Jwt:DurationInHours"])),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
