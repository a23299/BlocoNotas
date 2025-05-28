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
public class NoteSharesApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public NoteSharesApiController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/NoteSharesApi/shared-with-me
    [HttpGet("shared-with-me")]
    public async Task<ActionResult<IEnumerable<Note>>> GetNotesSharedWithMe()
    {
        var userId = GetCurrentUserId();
        
        var notes = await _context.Notes
            .Where(n => !n.IsDeleted)
            .Where(n => n.SharedWith.Any(ns => ns.NoteShareFK == userId))
            .Include(n => n.User) // Incluir o usuário que compartilhou
            .OrderByDescending(n => n.UpdatedAt)
            .ToListAsync();
            
        return notes;
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
        
        // Verifica se a nota pertence ao usuário atual
        var note = await _context.Notes
            .FirstOrDefaultAsync(n => n.NoteId == request.NoteId && n.UserFK == currentUserId && !n.IsDeleted);
            
        if (note == null)
        {
            return NotFound(new { message = "Nota não encontrada" });
        }
        
        // Verifica se o usuário com quem compartilhar existe
        var shareWithUser = await _context.Users
            .FirstOrDefaultAsync(u => u.UserName == request.ShareWithUsername);
            
        if (shareWithUser == null)
        {
            return BadRequest(new { message = "Usuário não encontrado" });
        }
        
        // Não permite compartilhar consigo mesmo
        if (shareWithUser.UserId == currentUserId)
        {
            return BadRequest(new { message = "Não é possível compartilhar uma nota consigo mesmo" });
        }
        
        // Verifica se já está compartilhado com este usuário
        var existingShare = await _context.NoteShares
            .FirstOrDefaultAsync(ns => ns.NoteShareFK == request.NoteId && ns.NoteShareFK == shareWithUser.UserId);
            
        if (existingShare != null)
        {
            return BadRequest(new { message = "Esta nota já está compartilhada com este usuário" });
        }
        
        // Cria o compartilhamento
        var noteShare = new NoteShare
        {
            NoteShareFK = request.NoteId,
            UserShareFK = shareWithUser.UserId,
            SharedAt = DateTime.Now,
            CanEdit = request.CanEdit
        };
        
        _context.NoteShares.Add(noteShare);
        await _context.SaveChangesAsync();
        
        // Inclui informações do usuário para retorno
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
                                     (ns.Note.UserFK == userId || ns.NoteShareFK == userId));
            
        if (noteShare == null)
        {
            return NotFound();
        }
        
        return noteShare;
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
        
        // Somente o proprietário da nota pode atualizar permissões
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
                                     (ns.Note.UserFK == userId || ns.NoteShareFK == userId));
            
        if (noteShare == null)
        {
            return NotFound();
        }
        
        _context.NoteShares.Remove(noteShare);
        await _context.SaveChangesAsync();
        
        return NoContent();
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return int.Parse(claim?.Value ?? "0");
    }
}

public class ShareNoteRequest
{
    public int NoteId { get; set; }
    public string ShareWithUsername { get; set; }
    public bool CanEdit { get; set; } = false;
}

public class UpdateShareRequest
{
    public bool CanEdit { get; set; }
}