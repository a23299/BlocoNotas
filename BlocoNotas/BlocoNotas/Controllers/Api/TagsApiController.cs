using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BlocoNotas.Data;
using BlocoNotas.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlocoNotas.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TagsApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TagsApiController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/TagsApi
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Tag>>> GetTags()
    {
        var userId = GetCurrentUserId();
        
        // Busca todas as tags que estão associadas a pelo menos uma nota do usuário atual
        var tags = await _context.Tags
            .Where(t => t.NoteTags.Any(nt => nt.Note.UserFK == userId && !nt.Note.IsDeleted))
            .OrderBy(t => t.Name)
            .ToListAsync();
            
        return tags;
    }

    // GET: api/TagsApi/all
    [HttpGet("all")]
    public async Task<ActionResult<IEnumerable<Tag>>> GetAllTags()
    {
        // Busca todas as tags disponíveis no sistema
        // Isso é útil quando o usuário quer associar uma tag existente a uma nota
        var tags = await _context.Tags
            .OrderBy(t => t.Name)
            .ToListAsync();
            
        return tags;
    }

    // GET: api/TagsApi/5
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

    // GET: api/TagsApi/5/notes
    [HttpGet("{id}/notes")]
    public async Task<ActionResult<IEnumerable<Note>>> GetNotesByTag(int id)
    {
        var userId = GetCurrentUserId();
        
        // Verifica se a tag existe
        var tag = await _context.Tags
            .FirstOrDefaultAsync(t => t.TagId == id);
            
        if (tag == null)
        {
            return NotFound();
        }
        
        // Busca as notas do usuário atual que têm essa tag
        var notes = await _context.Notes
            .Where(n => n.UserFK == userId && !n.IsDeleted)
            .Where(n => n.NoteTags.Any(nt => nt.TagFK == id))
            .OrderByDescending(n => n.UpdatedAt)
            .ToListAsync();
            
        return notes;
    }

    // POST: api/TagsApi
    [HttpPost]
    public async Task<ActionResult<Tag>> CreateTag(Tag tag)
    {
        // Verifica se já existe uma tag com o mesmo nome
        if (await _context.Tags.AnyAsync(t => t.Name == tag.Name))
        {
            return BadRequest(new { message = "Já existe uma tag com este nome" });
        }
        
        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTag), new { id = tag.TagId }, tag);
    }

    // PUT: api/TagsApi/5
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

        // Verifica se já existe outra tag com o mesmo nome
        if (await _context.Tags.AnyAsync(t => t.Name == tag.Name && t.TagId != id))
        {
            return BadRequest(new { message = "Já existe uma tag com este nome" });
        }

        // Atualiza apenas o nome da tag
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

    // DELETE: api/TagsApi/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTag(int id)
    {
        var tag = await _context.Tags
            .FirstOrDefaultAsync(t => t.TagId == id);

        if (tag == null)
        {
            return NotFound();
        }

        // Verificar se é seguro deletar (se outras pessoas usam)
        // Aqui você pode decidir se permite a exclusão apenas se nenhuma nota estiver usando a tag
        // ou sempre permite excluir

        // Remover todas as associações NoteTags antes de remover a tag
        var noteTags = await _context.NoteTags
            .Where(nt => nt.TagFK == id)
            .ToListAsync();
            
        _context.NoteTags.RemoveRange(noteTags);
        _context.Tags.Remove(tag);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // POST: api/TagsApi/notes
    [HttpPost("notes")]
    public async Task<IActionResult> AddTagToNote([FromBody] NoteTagRequest request)
    {
        var userId = GetCurrentUserId();
        
        // Verifica se a nota pertence ao usuário
        var note = await _context.Notes
            .FirstOrDefaultAsync(n => n.NoteId == request.NoteId && n.UserFK == userId && !n.IsDeleted);
            
        if (note == null)
        {
            return NotFound(new { message = "Nota não encontrada" });
        }
        
        // Verifica se a tag existe
        var tag = await _context.Tags
            .FirstOrDefaultAsync(t => t.TagId == request.TagId);
            
        if (tag == null)
        {
            return NotFound(new { message = "Tag não encontrada" });
        }
        
        // Verifica se a associação já existe
        if (await _context.NoteTags.AnyAsync(nt => nt.NoteTagFK == request.NoteId && nt.TagFK == request.TagId))
        {
            return BadRequest(new { message = "Esta tag já está associada à nota" });
        }
        
        // Cria a associação
        var noteTag = new NoteTag
        {
            NoteTagFK = request.NoteId,
            TagFK = request.TagId
        };
        
        _context.NoteTags.Add(noteTag);
        
        // Atualiza a data de atualização da nota
        note.UpdatedAt = DateTime.Now;
        
        await _context.SaveChangesAsync();
        
        return NoContent();
    }
    
    // DELETE: api/TagsApi/notes
    [HttpDelete("notes")]
    public async Task<IActionResult> RemoveTagFromNote([FromBody] NoteTagRequest request)
    {
        var userId = GetCurrentUserId();
        
        // Verifica se a nota pertence ao usuário
        var note = await _context.Notes
            .FirstOrDefaultAsync(n => n.NoteId == request.NoteId && n.UserFK == userId && !n.IsDeleted);
            
        if (note == null)
        {
            return NotFound(new { message = "Nota não encontrada" });
        }
        
        // Busca a associação
        var noteTag = await _context.NoteTags
            .FirstOrDefaultAsync(nt => nt.NoteTagFK == request.NoteId && nt.TagFK == request.TagId);
            
        if (noteTag == null)
        {
            return NotFound(new { message = "Esta tag não está associada à nota" });
        }
        
        _context.NoteTags.Remove(noteTag);
        
        // Atualiza a data de atualização da nota
        note.UpdatedAt = DateTime.Now;
        
        await _context.SaveChangesAsync();
        
        return NoContent();
    }

    private bool TagExists(int id)
    {
        return _context.Tags.Any(t => t.TagId == id);
    }

    private string GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
    }
}

public class NoteTagRequest
{
    public int NoteId { get; set; }
    public int TagId { get; set; }
}