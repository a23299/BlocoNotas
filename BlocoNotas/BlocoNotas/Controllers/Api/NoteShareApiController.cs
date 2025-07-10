using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BlocoNotas.Data;
using BlocoNotas.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlocoNotas.Controllers.Api
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/[controller]")]
    public class NoteSharesApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public NoteSharesApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/NoteSharesApi/shared-with-me
        [HttpGet("shared-with-me")]
        public async Task<ActionResult<IEnumerable<object>>> GetNotesSharedWithMe()
        {
            var userId = GetCurrentUserId();

            var notes = await _context.NoteShares
                .Where(ns => ns.UserShareFK == userId)
                .Include(ns => ns.Note)
                .ThenInclude(n => n.User)
                .Where(ns => ns.Note != null && !ns.Note.IsDeleted)
                .OrderByDescending(ns => ns.Note.UpdatedAt)
                .Select(ns => new
                {
                    noteId = ns.Note.NoteId,
                    title = ns.Note.Title,
                    content = ns.Note.Content,
                    updatedAt = ns.Note.UpdatedAt,
                    user = ns.Note.User == null ? null : new
                    {
                        userName = ns.Note.User.UserName
                    },
                    canEdit = ns.CanEdit
                })
                .ToListAsync();

            return Ok(notes);
        }

        // GET: api/NoteSharesApi/shared-by-me
        [HttpGet("shared-by-me")]
        public async Task<ActionResult<IEnumerable<NoteShare>>> GetMySharedNotes()
        {
            var userId = GetCurrentUserId();

            var shares = await _context.NoteShares
                .Where(ns => ns.Note.UserFK == userId && !ns.Note.IsDeleted)
                .Include(ns => ns.Note)
                .Include(ns => ns.SharedWithUser)
                .OrderByDescending(ns => ns.Note.UpdatedAt)
                .ToListAsync();

            return shares;
        }

        // POST: api/NoteSharesApi
        [HttpPost]
        public async Task<ActionResult<NoteShare>> ShareNote(ShareNoteRequest request)
        {
            var currentUserId = GetCurrentUserId();

            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(request.ShareWithUsername))
            {
                return BadRequest(new { message = "Nome de usuário é obrigatório" });
            }

            var note = await _context.Notes
                .FirstOrDefaultAsync(n => n.NoteId == request.NoteId && n.UserFK == currentUserId && !n.IsDeleted);

            if (note == null)
            {
                return NotFound(new { message = "Nota não encontrada" });
            }

            var shareWithUser = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == request.ShareWithUsername);

            if (shareWithUser == null)
            {
                return BadRequest(new { message = "Usuário não encontrado" });
            }

            if (shareWithUser.Id == currentUserId)
            {
                return BadRequest(new { message = "Não é possível compartilhar uma nota consigo mesmo" });
            }

            var existingShare = await _context.NoteShares
                .FirstOrDefaultAsync(ns => ns.NoteId == request.NoteId && ns.UserShareFK == shareWithUser.Id);

            if (existingShare != null)
            {
                return BadRequest(new { message = "Esta nota já está compartilhada com este usuário" });
            }

            var noteShare = new NoteShare
            {
                NoteId = request.NoteId,
                UserShareFK = shareWithUser.Id,
                SharedAt = DateTime.Now,
                CanEdit = request.CanEdit
            };

            _context.NoteShares.Add(noteShare);
            await _context.SaveChangesAsync();

            // Incluir dados para retorno
            noteShare.SharedWithUser = shareWithUser;
            noteShare.Note = note;

            return CreatedAtAction(nameof(GetShareDetails), new { id = noteShare.NoteShareId }, noteShare);
        }

        // GET: api/NoteSharesApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<NoteShare>> GetShareDetails(int id)
        {
            var userId = GetCurrentUserId();

            var noteShare = await _context.NoteShares
                .Include(ns => ns.Note)
                .Include(ns => ns.SharedWithUser)
                .FirstOrDefaultAsync(ns => ns.NoteShareId == id &&
                                           (ns.Note.UserFK == userId || ns.UserShareFK == userId));

            if (noteShare == null)
            {
                return NotFound();
            }

            return noteShare;
        }

        // PUT: api/NoteSharesApi/shared-note-edit/{noteId}
        [HttpPut("shared-note-edit/{noteId}")]
        public async Task<IActionResult> EditSharedNote(int noteId, [FromBody] NoteEditRequest request)
        {
            var userId = GetCurrentUserId();

            // Verifica permissão de edição
            var share = await _context.NoteShares
                .Include(ns => ns.Note)
                .FirstOrDefaultAsync(ns => ns.NoteId == noteId && ns.UserShareFK == userId && ns.CanEdit);

            if (share == null || share.Note == null || share.Note.IsDeleted)
            {
                return NotFound(new { message = "Nota não encontrada ou sem permissão para editar" });
            }

            share.Note.Title = request.Title;
            share.Note.Content = request.Content;
            share.Note.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PUT: api/NoteSharesApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNoteShare(int id, UpdateShareRequest request)
        {
            var userId = GetCurrentUserId();

            var noteShare = await _context.NoteShares
                .Include(ns => ns.Note)
                .FirstOrDefaultAsync(ns => ns.NoteShareId == id && ns.Note.UserFK == userId);

            if (noteShare == null)
            {
                return NotFound();
            }

            // Só o dono da nota pode alterar permissões
            noteShare.CanEdit = request.CanEdit;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/NoteSharesApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNoteShare(int id)
        {
            var userId = GetCurrentUserId();

            var noteShare = await _context.NoteShares
                .Include(ns => ns.Note)
                .FirstOrDefaultAsync(ns => ns.NoteShareId == id &&
                                           (ns.Note.UserFK == userId || ns.UserShareFK == userId));

            if (noteShare == null)
            {
                return NotFound();
            }

            _context.NoteShares.Remove(noteShare);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private string GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim?.Value ?? string.Empty;
        }
    }

    // Request para partilha
    public class ShareNoteRequest
    {
        public int NoteId { get; set; }
        public string ShareWithUsername { get; set; }
        public bool CanEdit { get; set; } = false;
    }

    // Request para atualizar permissão de edição
    public class UpdateShareRequest
    {
        public bool CanEdit { get; set; }
    }

    // Request para editar a nota (conteúdo e título)
    public class NoteEditRequest
    {
        public string Title { get; set; }
        public string Content { get; set; }
    }
}
