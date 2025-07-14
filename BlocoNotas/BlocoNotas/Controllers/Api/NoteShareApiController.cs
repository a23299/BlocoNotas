using System.Security.Claims;
using BlocoNotas.Data;
using BlocoNotas.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlocoNotas.Controllers.Api;

/// <summary>
/// Controlador da API responsável por gerenciar as partilhas de notas.
/// </summary>
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

    /// <summary>
    /// Obtém as notas que foram partilhadas com o utilizador atual.
    /// </summary>
    /// <returns>Lista de notas partilhadas.</returns>
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

    /// <summary>
    /// Obtém as notas que o utilizador atual partilhou com outros.
    /// </summary>
    /// <returns>Lista de partilhas de notas.</returns>
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

    /// <summary>
    /// Partilha uma nota com outro utilizador.
    /// </summary>
    /// <param name="request">Dados da partilha.</param>
    /// <returns>Informações da partilha criada.</returns>
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

        noteShare.SharedWithUser = shareWithUser;
        noteShare.Note = note;

        return CreatedAtAction(nameof(GetShareDetails), new { id = noteShare.NoteShareId }, noteShare);
    }

    /// <summary>
    /// Remove o acesso do utilizador atual a uma nota partilhada.
    /// </summary>
    /// <param name="noteId">ID da nota.</param>
    /// <returns>Resultado da operação.</returns>
    [HttpDelete("remove-my-access/{noteId}")]
    public async Task<IActionResult> RemoveMyAccess(int noteId)
    {
        var userId = GetCurrentUserId();

        var noteShare = await _context.NoteShares
            .FirstOrDefaultAsync(ns => ns.NoteId == noteId && ns.UserShareFK == userId);

        if (noteShare == null)
        {
            return NotFound(new { message = "Partilha não encontrada." });
        }

        _context.NoteShares.Remove(noteShare);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Obtém os detalhes de uma partilha de nota específica.
    /// </summary>
    /// <param name="id">ID da partilha.</param>
    /// <returns>Detalhes da partilha.</returns>
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

    /// <summary>
    /// Edita o conteúdo de uma nota partilhada (caso o utilizador tenha permissão de edição).
    /// </summary>
    /// <param name="noteId">ID da nota.</param>
    /// <param name="request">Conteúdo atualizado.</param>
    /// <returns>Resultado da operação.</returns>
    [HttpPut("shared-note-edit/{noteId}")]
    public async Task<IActionResult> EditSharedNote(int noteId, [FromBody] NoteEditRequest request)
    {
        var userId = GetCurrentUserId();

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

    /// <summary>
    /// Atualiza permissões de edição de uma partilha.
    /// </summary>
    /// <param name="id">ID da partilha.</param>
    /// <param name="request">Dados da atualização.</param>
    /// <returns>Resultado da operação.</returns>
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

        noteShare.CanEdit = request.CanEdit;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Exclui uma partilha de nota.
    /// </summary>
    /// <param name="id">ID da partilha.</param>
    /// <returns>Resultado da operação.</returns>
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

    /// <summary>
    /// Obtém o ID do utilizador atualmente autenticado.
    /// </summary>
    /// <returns>ID do utilizador.</returns>
    private string GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim?.Value ?? string.Empty;
    }
}

/// <summary>
/// Modelo para solicitação de partilha de nota.
/// </summary>
public class ShareNoteRequest
{
    public int NoteId { get; set; }
    public string ShareWithUsername { get; set; }
    public bool CanEdit { get; set; } = false;
}

/// <summary>
/// Modelo para solicitação de atualização de permissões de partilha.
/// </summary>
public class UpdateShareRequest
{
    public bool CanEdit { get; set; }
}

/// <summary>
/// Modelo para edição de nota.
/// </summary>
public class NoteEditRequest
{
    public string Title { get; set; }
    public string Content { get; set; }
}
