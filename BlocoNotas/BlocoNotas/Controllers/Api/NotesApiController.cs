using System.Security.Claims;
using BlocoNotas.Data;
using BlocoNotas.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlocoNotas.Controllers.Api
{
    /// <summary>
    /// Controlador API para gerir notas com autenticação JWT.
    /// </summary>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/[controller]")]
    public class NotesApiController : ControllerBase // Note ControllerBase em vez de Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        /// <summary>
        /// Construtor para injeção de dependências.
        /// </summary>
        /// <param name="context">Contexto da base de dados.</param>
        /// <param name="userManager">Gerenciador de utilizadores.</param>
        public NotesApiController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Obtém todas as notas do utilizador atual ou todas as notas para administradores.
        /// </summary>
        /// <returns>Lista de notas.</returns>
        [HttpGet]
        public async Task<ActionResult> GetNotes()
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Autenticação requerida." });

            var isAdmin = User.IsInRole("Admin");

            if (isAdmin)
            {
                // Admin: listar todas as notas organizadas por utilizador
                var notesByUser = await _context.Notes
                    .Where(n => !n.IsDeleted)
                    .Include(n => n.NoteTags)
                    .ThenInclude(nt => nt.Tag)
                    .Include(n => n.User)
                    .GroupBy(n => new { n.User.Id, n.User.UserName })
                    .Select(group => new
                    {
                        userId = group.Key.Id,
                        userName = group.Key.UserName,
                        notes = group
                            .Select(n => new
                            {
                                n.NoteId,
                                n.Title,
                                n.Content,
                                n.UpdatedAt,
                                tags = n.NoteTags.Select(nt => nt.Tag.Name).ToList()
                            })
                            .OrderByDescending(n => n.UpdatedAt)
                            .ToList()
                    })
                    .ToListAsync();

                return Ok(notesByUser);
            }
            else
            {
                // Utilizador normal: só as suas notas
                var notes = await _context.Notes
                    .Where(n => n.UserFK == userId && !n.IsDeleted)
                    .Include(n => n.NoteTags)
                    .ThenInclude(nt => nt.Tag)
                    .OrderByDescending(n => n.UpdatedAt)
                    .Select(n => new
                    {
                        n.NoteId,
                        n.Title,
                        n.Content,
                        n.UpdatedAt,
                        tags = n.NoteTags.Select(nt => nt.Tag.Name)
                    })
                    .ToListAsync();

                return Ok(notes);
            }
        }

        /// <summary>
        /// Obtém uma nota específica pelo seu ID.
        /// </summary>
        /// <param name="id">ID da nota.</param>
        /// <returns>Nota solicitada.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Note>> GetNote(int id)
        {
            var userId = GetCurrentUserId();
            var isAdmin = User.IsInRole("Admin");

            Note? note;

            if (isAdmin)
            {
                note = await _context.Notes
                    .Where(n => n.NoteId == id && !n.IsDeleted)
                    .Include(n => n.NoteTags).ThenInclude(nt => nt.Tag)
                    .FirstOrDefaultAsync();
            }
            else
            {
                note = await _context.Notes
                    .Where(n => n.NoteId == id && n.UserFK == userId && !n.IsDeleted)
                    .Include(n => n.NoteTags).ThenInclude(nt => nt.Tag)
                    .FirstOrDefaultAsync();
            }

            if (note == null)
                return NotFound();

            return note;
        }

        /// <summary>
        /// Cria uma nova nota para o utilizador autenticado.
        /// </summary>
        /// <param name="note">Dados da nota a criar.</param>
        /// <returns>Nota criada.</returns>
        [HttpPost]
        public async Task<ActionResult<Note>> CreateNote([FromBody] Note note)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Autenticação requerida." });
            }

            note.UserFK = userId;
            note.CreatedAt = DateTime.Now;
            note.UpdatedAt = DateTime.Now;
            note.IsDeleted = false;

            _context.Notes.Add(note);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetNote), new { id = note.NoteId }, note);
        }

        /// <summary>
        /// Atualiza uma nota existente.
        /// </summary>
        /// <param name="id">ID da nota a atualizar.</param>
        /// <param name="note">Dados atualizados da nota.</param>
        /// <returns>Resposta de sucesso ou erro.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNote(int id, Note note)
        {
            if (id != note.NoteId)
            {
                return BadRequest();
            }

            var userId = GetCurrentUserId();
            var isAdmin = User.IsInRole("Admin");

            var existingNote = await _context.Notes
                .FirstOrDefaultAsync(n => n.NoteId == id && n.UserFK == userId && !n.IsDeleted);

            if (isAdmin)
            {
                existingNote = await _context.Notes
                    .FirstOrDefaultAsync(n => n.NoteId == id && !n.IsDeleted);
            }
            else
            {
                existingNote = await _context.Notes
                    .FirstOrDefaultAsync(n => n.NoteId == id && n.UserFK == userId && !n.IsDeleted);
            }

            if (existingNote == null)
            {
                return NotFound();
            }

            // Atualizar apenas as propriedades permitidas
            existingNote.Title = note.Title;
            existingNote.Content = note.Content;
            existingNote.UpdatedAt = DateTime.Now;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NoteExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        /// <summary>
        /// Elimina logicamente uma nota pelo seu ID.
        /// </summary>
        /// <param name="id">ID da nota a eliminar.</param>
        /// <returns>Resposta de sucesso ou erro.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNote(int id)
        {
            var userId = GetCurrentUserId();
            var isAdmin = User.IsInRole("Admin");

            var note = await _context.Notes
                .FirstOrDefaultAsync(n => n.NoteId == id && n.UserFK == userId && !n.IsDeleted);

            if (isAdmin)
            {
                // Admin pode apagar qualquer nota não eliminada
                note = await _context.Notes
                    .FirstOrDefaultAsync(n => n.NoteId == id && !n.IsDeleted);
            }
            else
            {
                // Utilizador normal só pode apagar as suas notas
                note = await _context.Notes
                    .FirstOrDefaultAsync(n => n.NoteId == id && n.UserFK == userId && !n.IsDeleted);
            }

            if (note == null)
            {
                return NotFound();
            }

            // Exclusão lógica
            note.IsDeleted = true;
            note.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Verifica se uma nota existe e pertence ao utilizador ou é visível para admin.
        /// </summary>
        /// <param name="id">ID da nota.</param>
        /// <returns>True se existir e não estiver eliminada, caso contrário false.</returns>
        private bool NoteExists(int id)
        {
            var userId = GetCurrentUserId();
            var isAdmin = User.IsInRole("Admin");

            if (isAdmin)
                return _context.Notes.Any(e => e.NoteId == id && !e.IsDeleted);
            else
                return _context.Notes.Any(e => e.NoteId == id && e.UserFK == userId && !e.IsDeleted);
        }

        /// <summary>
        /// Obtém o ID do utilizador autenticado a partir dos claims.
        /// </summary>
        /// <returns>ID do utilizador ou string vazia se não autenticado.</returns>
        private string GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim?.Value ?? string.Empty;  // Retorna string ou string vazia
        }
    }
}
