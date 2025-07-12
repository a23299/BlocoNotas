using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlocoNotas.Data;
using BlocoNotas.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace BlocoNotas.Controllers.Api
{
    /// <summary>
    /// Controller para gerenciar operações relacionadas aos usuários via API.
    /// </summary>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        /// <summary>
        /// Construtor da controller que injeta o contexto do banco e o UserManager.
        /// </summary>
        /// <param name="context">Contexto da aplicação para acesso ao banco.</param>
        /// <param name="userManager">Gerenciador de usuários do Identity.</param>
        public UsersApiController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Obtém a lista de usuários.
        /// - Admins recebem todos os usuários com seus papéis.
        /// - Usuários normais recebem apenas seus próprios dados com papéis.
        /// </summary>
        /// <returns>Lista de usuários com seus papéis.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetUsers()
        {
            if (User.IsInRole("Admin"))
            {
                var users = await _context.Users.ToListAsync();

                var usersWithRoles = new List<object>();

                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    usersWithRoles.Add(new
                    {
                        id = user.Id,
                        userName = user.UserName,
                        email = user.Email,
                        roles = roles
                    });
                }

                return Ok(usersWithRoles);
            }
            else
            {
                // Usuário normal: retorna apenas os dados dele próprio
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var user = await _context.Users.FindAsync(currentUserId);
                if (user == null)
                    return NotFound();

                var roles = await _userManager.GetRolesAsync(user);

                var userWithRoles = new List<object>
                {
                    new
                    {
                        id = user.Id,
                        userName = user.UserName,
                        email = user.Email,
                        roles = roles
                    }
                };

                return Ok(userWithRoles);
            }
        }

        /// <summary>
        /// Obtém os detalhes de um usuário específico pelo seu ID.
        /// Apenas admins ou o próprio usuário podem acessar.
        /// </summary>
        /// <param name="id">ID do usuário a ser obtido.</param>
        /// <returns>Dados do usuário solicitado.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApplicationUser>> GetUser(string id)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!User.IsInRole("Admin") && currentUserId != id)
                return Forbid(); // Só admin ou dono podem ver

            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        /// <summary>
        /// Cria um novo usuário.
        /// Apenas admins podem criar novos usuários.
        /// </summary>
        /// <param name="applicationUser">Objeto do usuário a ser criado.</param>
        /// <returns>Usuário criado.</returns>
        [HttpPost]
        public async Task<ActionResult<ApplicationUser>> PostUser(ApplicationUser applicationUser)
        {
            if (!User.IsInRole("Admin"))
                return Forbid();

            _context.Users.Add(applicationUser);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUser", new { id = applicationUser.Id }, applicationUser);
        }

        /// <summary>
        /// Atualiza um usuário existente.
        /// Apenas admins ou o próprio usuário podem atualizar.
        /// Permite alteração de dados e senha.
        /// </summary>
        /// <param name="id">ID do usuário a ser atualizado.</param>
        /// <param name="applicationUser">Objeto com dados atualizados do usuário.</param>
        /// <returns>Resultado da operação.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(string id, ApplicationUser applicationUser)
        {
            if (id != applicationUser.Id)
                return BadRequest();

            if (!User.IsInRole("Admin") && User.FindFirst(ClaimTypes.NameIdentifier)?.Value != id)
                return Forbid();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            user.UserName = applicationUser.UserName;
            user.Email = applicationUser.Email;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return BadRequest(updateResult.Errors);

            if (!string.IsNullOrEmpty(applicationUser.Password))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var passwordResult = await _userManager.ResetPasswordAsync(user, token, applicationUser.Password);
                if (!passwordResult.Succeeded)
                    return BadRequest(passwordResult.Errors);
            }

            return NoContent();
        }

        /// <summary>
        /// Remove um usuário específico.
        /// Apenas admins podem deletar usuários.
        /// Apaga também notas e compartilhamentos associados ao usuário.
        /// </summary>
        /// <param name="id">ID do usuário a ser deletado.</param>
        /// <returns>Resultado da operação.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (!User.IsInRole("Admin"))
                return Forbid();

            var user = await _context.Users
                .Include(u => u.Notes)
                .ThenInclude(n => n.SharedWith)
                .Include(u => u.SharedWithMe)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            // Apagar compartilhamentos recebidos pelo usuário
            if (user.SharedWithMe != null && user.SharedWithMe.Any())
            {
                _context.NoteShares.RemoveRange(user.SharedWithMe);
            }

            // Apagar compartilhamentos feitos nas notas do usuário
            if (user.Notes != null)
            {
                foreach (var note in user.Notes)
                {
                    if (note.SharedWith != null && note.SharedWith.Any())
                    {
                        _context.NoteShares.RemoveRange(note.SharedWith);
                    }
                }
            }

            // Apagar notas do usuário
            if (user.Notes != null && user.Notes.Any())
            {
                _context.Notes.RemoveRange(user.Notes);
            }

            // Salvar alterações antes de apagar o usuário
            await _context.SaveChangesAsync();

            // Apagar o usuário propriamente dito
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest("Erro ao eliminar o utilizador.");
            }

            return NoContent();
        }

        /// <summary>
        /// Promove um usuário para o papel de Admin.
        /// Apenas admins podem promover.
        /// </summary>
        /// <param name="id">ID do usuário a ser promovido.</param>
        /// <returns>Resultado da operação.</returns>
        [HttpPost("MakeAdmin/{id}")]
        public async Task<IActionResult> MakeAdmin(string id)
        {
            if (!User.IsInRole("Admin"))
                return Forbid();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            if (await _userManager.IsInRoleAsync(user, "Admin"))
                return BadRequest("Utilizador já é admin");

            var result = await _userManager.AddToRoleAsync(user, "Admin");
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = "Utilizador promovido a Admin com sucesso." });
        }

        /// <summary>
        /// Verifica se um usuário existe no banco.
        /// </summary>
        /// <param name="id">ID do usuário a verificar.</param>
        /// <returns>True se existir, false caso contrário.</returns>
        private bool UserExists(string id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
