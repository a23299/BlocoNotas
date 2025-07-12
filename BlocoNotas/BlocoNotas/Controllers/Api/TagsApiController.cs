using System.Security.Claims;
using BlocoNotas.Data;
using BlocoNotas.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlocoNotas.Controllers.Api;

/// <summary>
/// Controller API para gerenciar operações relacionadas a Tags.
/// </summary>
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public class TagsApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Construtor que recebe o contexto do banco de dados.
    /// </summary>
    /// <param name="context">Contexto do banco de dados.</param>
    public TagsApiController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtém todas as tags associadas a pelo menos uma nota do usuário atual.
    /// </summary>
    /// <returns>Lista de tags do usuário.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Tag>>> GetTags()
    {
        var userId = GetCurrentUserId();
        
        var tags = await _context.Tags
            .Where(t => t.NoteTags.Any(nt => nt.Note.UserFK == userId && !nt.Note.IsDeleted))
            .OrderBy(t => t.Name)
            .ToListAsync();
            
        return tags;
    }

    /// <summary>
    /// Obtém todas as tags existentes no sistema.
    /// </summary>
    /// <returns>Lista completa de tags.</returns>
    [HttpGet("all")]
    public async Task<ActionResult<IEnumerable<Tag>>> GetAllTags()
    {
        var tags = await _context.Tags
            .OrderBy(t => t.Name)
            .ToListAsync();
            
        return tags;
    }

    /// <summary>
    /// Obtém uma tag pelo seu ID.
    /// </summary>
    /// <param name="id">ID da tag.</param>
    /// <returns>Tag encontrada ou NotFound.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<Tag>> GetTag(int id)
    {
        var tag = await _context.Tags
            .FirstOrDefaultAsync(t => t.TagId == id);

        if (tag == null)
        {
            return NotFound();
        }

        return tag;
    }

    /// <summary>
    /// Obtém as notas associadas a uma tag específica para o usuário atual.
    /// </summary>
    /// <param name="id">ID da tag.</param>
    /// <returns>Lista de notas associadas.</returns>
    [HttpGet("{id}/notes")]
    public async Task<ActionResult<IEnumerable<Note>>> GetNotesByTag(int id)
    {
        var userId = GetCurrentUserId();
        
        var tag = await _context.Tags
            .FirstOrDefaultAsync(t => t.TagId == id);
            
        if (tag == null)
        {
            return NotFound();
        }
        
        var notes = await _context.Notes
            .Where(n => n.UserFK == userId && !n.IsDeleted)
            .Where(n => n.NoteTags.Any(nt => nt.TagFK == id))
            .OrderByDescending(n => n.UpdatedAt)
            .ToListAsync();
            
        return notes;
    }

    /// <summary>
    /// Cria uma nova tag.
    /// </summary>
    /// <param name="tag">Objeto Tag com os dados da nova tag.</param>
    /// <returns>Tag criada ou erro se nome já existir.</returns>
    [HttpPost]
    public async Task<ActionResult<Tag>> CreateTag(Tag tag)
    {
        if (await _context.Tags.AnyAsync(t => t.Name == tag.Name))
        {
            return BadRequest(new { message = "Já existe uma tag com este nome" });
        }
        
        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTag), new { id = tag.TagId }, tag);
    }

    /// <summary>
    /// Atualiza o nome de uma tag existente.
    /// </summary>
    /// <param name="id">ID da tag a ser atualizada.</param>
    /// <param name="tag">Objeto Tag com os novos dados.</param>
    /// <returns>Status da operação.</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTag(int id, Tag tag)
    {
        if (id != tag.TagId)
        {
            return BadRequest();
        }

        var existingTag = await _context.Tags
            .FirstOrDefaultAsync(t => t.TagId == id);

        if (existingTag == null)
        {
            return NotFound();
        }

        if (await _context.Tags.AnyAsync(t => t.Name == tag.Name && t.TagId != id))
        {
            return BadRequest(new { message = "Já existe uma tag com este nome" });
        }

        existingTag.Name = tag.Name;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!TagExists(id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    /// <summary>
    /// Remove uma tag pelo seu ID, incluindo suas associações com notas.
    /// </summary>
    /// <param name="id">ID da tag a ser removida.</param>
    /// <returns>Status da operação.</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTag(int id)
    {
        var tag = await _context.Tags
            .FirstOrDefaultAsync(t => t.TagId == id);

        if (tag == null)
        {
            return NotFound();
        }

        var noteTags = await _context.NoteTags
            .Where(nt => nt.TagFK == id)
            .ToListAsync();
            
        _context.NoteTags.RemoveRange(noteTags);
        _context.Tags.Remove(tag);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Associa uma tag a uma nota, se o usuário tiver permissão.
    /// </summary>
    /// <param name="request">Objeto contendo NoteId e TagId para associação.</param>
    /// <returns>Status da operação.</returns>
    [HttpPost("notes")]
    public async Task<IActionResult> AddTagToNote([FromBody] NoteTagRequest request)
    {
        var userId = GetCurrentUserId();
        var isAdmin = User.IsInRole("Admin");

        var note = await _context.Notes
            .FirstOrDefaultAsync(n => n.NoteId == request.NoteId && !n.IsDeleted && (isAdmin || n.UserFK == userId));

        if (note == null)
        {
            return NotFound(new { message = "Nota não encontrada ou sem permissão." });
        }

        var tag = await _context.Tags
            .FirstOrDefaultAsync(t => t.TagId == request.TagId);

        if (tag == null)
        {
            return NotFound(new { message = "Tag não encontrada" });
        }

        if (await _context.NoteTags.AnyAsync(nt => nt.NoteTagFK == request.NoteId && nt.TagFK == request.TagId))
        {
            return BadRequest(new { message = "Esta tag já está associada à nota" });
        }

        var noteTag = new NoteTag
        {
            NoteTagFK = request.NoteId,
            TagFK = request.TagId
        };

        _context.NoteTags.Add(noteTag);

        note.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();

        return NoContent();
    }
    
    /// <summary>
    /// Remove a associação entre uma tag e uma nota, respeitando permissões.
    /// </summary>
    /// <param name="request">Objeto contendo NoteId e TagId para remoção da associação.</param>
    /// <returns>Status da operação.</returns>
    [HttpDelete("notes")]
    public async Task<IActionResult> RemoveTagFromNote([FromBody] NoteTagRequest request)
    {
        var userId = GetCurrentUserId();
        var isAdmin = User.IsInRole("Admin");

        var note = await _context.Notes
            .FirstOrDefaultAsync(n => n.NoteId == request.NoteId && !n.IsDeleted && (isAdmin || n.UserFK == userId));

        if (note == null)
        {
            return NotFound(new { message = "Nota não encontrada ou sem permissão." });
        }

        var noteTag = await _context.NoteTags
            .FirstOrDefaultAsync(nt => nt.NoteTagFK == request.NoteId && nt.TagFK == request.TagId);

        if (noteTag == null)
        {
            return NotFound(new { message = "Esta tag não está associada à nota" });
        }

        _context.NoteTags.Remove(noteTag);

        note.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Verifica se uma tag existe pelo seu ID.
    /// </summary>
    /// <param name="id">ID da tag.</param>
    /// <returns>True se existir, false caso contrário.</returns>
    private bool TagExists(int id)
    {
        return _context.Tags.Any(t => t.TagId == id);
    }

    /// <summary>
    /// Obtém o ID do usuário atual autenticado.
    /// </summary>
    /// <returns>ID do usuário ou string vazia se não encontrado.</returns>
    private string GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
    }
}

/// <summary>
/// Modelo para requisições de associação/desassociação entre notas e tags.
/// </summary>
public class NoteTagRequest
{
    /// <summary>
    /// ID da nota.
    /// </summary>
    public int NoteId { get; set; }

    /// <summary>
    /// ID da tag.
    /// </summary>
    public int TagId { get; set; }
}
