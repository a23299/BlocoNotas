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
[Authorize] // Para proteger com JWT
public class NotesApiController : ControllerBase // Note ControllerBase em vez de Controller
{
    private readonly ApplicationDbContext _context;

    public NotesApiController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/NotesApi
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Note>>> GetNotes()
    {
        if (User.Identity?.IsAuthenticated ?? false)
        {
            var userId = GetCurrentUserId();
            return await _context.Notes
                .Where(n => n.UserFK == userId && !n.IsDeleted)
                .OrderByDescending(n => n.UpdatedAt)
                .ToListAsync();
        }
        else
        {
            // Estamos a mostrar TODAS as notas não eliminadas
            // mesmo que pertençam a outros users
            return await _context.Notes
                .Where(n => !n.IsDeleted)
                .OrderByDescending(n => n.UpdatedAt)
                .ToListAsync();
        }
    }

    // GET: api/NotesApi/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Note>> GetNote(int id)
    {
        var userId = GetCurrentUserId();
        var note = await _context.Notes
            .FirstOrDefaultAsync(n => n.NoteId == id && n.UserFK == userId && !n.IsDeleted);

        if (note == null)
        {
            return NotFound();
        }

        return note;
    }

    // POST: api/NotesApi
    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<Note>> CreateNote(Note note)
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim != null)
        {
            // Utilizador autenticado
            note.UserFK = int.Parse(claim.Value);
        }
        else if (note.UserFK == 0)
        {
            return BadRequest(new { message = "UserFK é obrigatório se não estiver autenticado." });
        }

        note.CreatedAt = DateTime.Now;
        note.UpdatedAt = DateTime.Now;
        note.IsDeleted = false;

        _context.Notes.Add(note);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetNote), new { id = note.NoteId }, note);
    }

    // PUT: api/NotesApi/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateNote(int id, Note note)
    {
        if (id != note.NoteId)
        {
            return BadRequest();
        }

        var userId = GetCurrentUserId();
        var existingNote = await _context.Notes
            .FirstOrDefaultAsync(n => n.NoteId == id && n.UserFK == userId && !n.IsDeleted);

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

    // DELETE: api/NotesApi/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNote(int id)
    {
        var userId = GetCurrentUserId();
        var note = await _context.Notes
            .FirstOrDefaultAsync(n => n.NoteId == id && n.UserFK == userId && !n.IsDeleted);

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

    private bool NoteExists(int id)
    {
        var userId = GetCurrentUserId();
        return _context.Notes.Any(e => e.NoteId == id && e.UserFK == userId && !e.IsDeleted);
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return int.Parse(claim?.Value ?? "0");
    }
}