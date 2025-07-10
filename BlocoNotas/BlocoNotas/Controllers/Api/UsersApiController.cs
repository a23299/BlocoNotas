using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlocoNotas.Data;
using BlocoNotas.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
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
        public async Task<ActionResult<IEnumerable<ApplicationUser>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        // GET: api/UsersApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ApplicationUser>> GetUser(string id)
        {
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
            _context.Users.Add(applicationUser);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUser", new { id = applicationUser.Id }, applicationUser);
        }

        // PUT: api/UsersApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(string id, ApplicationUser applicationUser)
        {
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
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

        private bool UserExists(string id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
