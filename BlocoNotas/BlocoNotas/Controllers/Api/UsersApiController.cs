using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlocoNotas.Data;
using BlocoNotas.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace BlocoNotas.Controllers.Api
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersApiController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        
        
        // GET: api/UsersApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetUsers()
        {
            if (User.IsInRole("Admin"))
            {
                var users = await _context.Users.ToListAsync();

                var usersWithRoles = new List<object>();

                foreach(var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    usersWithRoles.Add(new {
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
                // User normal: só o próprio
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var user = await _context.Users.FindAsync(currentUserId);
                if (user == null)
                    return NotFound();

                var roles = await _userManager.GetRolesAsync(user);

                var userWithRoles = new List<object> {
                    new {
                        id = user.Id,
                        userName = user.UserName,
                        email = user.Email,
                        roles = roles
                    }
                };

                return Ok(userWithRoles);
            }
        }



        // GET: api/UsersApi/5
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

        // POST: api/UsersApi
        [HttpPost]
        public async Task<ActionResult<ApplicationUser>> PostUser(ApplicationUser applicationUser)
        {
            
            if (!User.IsInRole("Admin"))
                return Forbid();
            
            _context.Users.Add(applicationUser);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUser", new { id = applicationUser.Id }, applicationUser);
        }

        // PUT: api/UsersApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(string id, ApplicationUser applicationUser)
        {
            
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!User.IsInRole("Admin") && currentUserId != id)
                return Forbid();
            
            if (id != applicationUser.Id)
            {
                return BadRequest();
            }

            _context.Entry(applicationUser).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        
        // DELETE
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

            // Apagar partilhas feitas COM o user (onde o user é o destinatário)
            if (user.SharedWithMe != null && user.SharedWithMe.Any())
            {
                _context.NoteShares.RemoveRange(user.SharedWithMe);
            }

            // Apagar partilhas feitas NAS notas do user (partilhas feitas PELO user)
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

            // Apagar notas do user
            if (user.Notes != null && user.Notes.Any())
            {
                _context.Notes.RemoveRange(user.Notes);
            }

            // Salvar alterações antes de apagar o user
            await _context.SaveChangesAsync();

            // Agora apagar o user
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest("Erro ao eliminar o utilizador.");
            }

            return NoContent();
        }
        
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


        private bool UserExists(string id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
