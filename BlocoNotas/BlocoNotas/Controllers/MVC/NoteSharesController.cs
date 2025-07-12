using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BlocoNotas.Data;
using BlocoNotas.Models;


namespace BlocoNotas.Controllers.MVC;

// Remove [Authorize] temporarily to test
public class NoteSharesController : Controller
{
    private readonly ApplicationDbContext _context;

    public NoteSharesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: NoteShares
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Console.WriteLine($"[DEBUG] userId = {userId}");

        
        


        var sharedByMe = await _context.NoteShares
            .Where(ns => ns.Note.UserFK == userId && !ns.Note.IsDeleted)
            .Include(ns => ns.Note)
            .Include(ns => ns.SharedWithUser)
            .ToListAsync();

        var sharedWithMeNoteShares = await _context.NoteShares
            .Where(ns => ns.UserShareFK == userId && !ns.Note.IsDeleted)
            .Include(ns => ns.Note)
            .ToListAsync();

        var sharedWithMe = new List<Note>();

        foreach (var ns in sharedWithMeNoteShares)
        {
            // Carrega explicitamente o dono da nota (User)
            await _context.Entry(ns.Note).Reference(n => n.User).LoadAsync();
            sharedWithMe.Add(ns.Note);
        }

        // Debug console output
        Console.WriteLine($"UserId: {userId}");
        Console.WriteLine($"Notas partilhadas por mim: {sharedByMe.Count}");
        Console.WriteLine($"Notas partilhadas comigo: {sharedWithMe.Count}");

        foreach (var share in sharedByMe)
        {
            Console.WriteLine($"SharedByMe: NoteShareId={share.NoteShareId}, NoteTitle={share.Note?.Title}, SharedWith={share.SharedWithUser?.UserName}");
        }

        foreach (var note in sharedWithMe)
        {
            Console.WriteLine($"SharedWithMe: NoteId={note.NoteId}, Title={note.Title}, Owner={note.User?.UserName}");
        }
        
        return View();
    }
    // GET: NoteShares/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var noteShare = await _context.NoteShares
            .Include(n => n.Note)
            .Include(n => n.SharedWithUser)
            .FirstOrDefaultAsync(m => m.NoteShareId == id && 
                                     (m.Note.UserFK == userId || m.UserShareFK == userId));
        
        if (noteShare == null)
        {
            return NotFound();
        }

        return View(noteShare);
    }

    // GET: NoteShares/Create
    public async Task<IActionResult> Create()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }
        
        // Only show notes owned by the current user
        var userNotes = await _context.Notes
            .Where(n => n.UserFK == userId && !n.IsDeleted)
            .ToListAsync();
            
        ViewData["NoteShareFK"] = new SelectList(userNotes, "NoteId", "Title");
        ViewData["UserShareFK"] = new SelectList(_context.Users, "Id", "UserName");
        return View();
    }

    // POST: NoteShares/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("NoteShareFK,UserShareFK,CanEdit")] NoteShare noteShare)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        // Verify the note belongs to the current user
        var note = await _context.Notes
            .FirstOrDefaultAsync(n => n.NoteId == noteShare.NoteId && n.UserFK == userId && !n.IsDeleted);
            
        if (note == null)
        {
            ModelState.AddModelError("", "Nota não encontrada ou não pertence a você.");
            await SetupCreateViewData(userId);
            return View(noteShare);
        }
        
        // Check if user is trying to share with themselves
        if (noteShare.UserShareFK == userId)
        {
            ModelState.AddModelError("", "Não é possível partilhar uma nota consigo mesmo.");
            await SetupCreateViewData(userId);
            return View(noteShare);
        }
        
        // Check if already shared with this user
        var existingShare = await _context.NoteShares
            .FirstOrDefaultAsync(ns => ns.NoteId == noteShare.NoteId && 
                                      ns.UserShareFK == noteShare.UserShareFK);
        
        if (existingShare != null)
        {
            ModelState.AddModelError("", "Esta nota já está partilhada com este utilizador.");
            await SetupCreateViewData(userId);
            return View(noteShare);
        }

        if (ModelState.IsValid)
        {
            noteShare.SharedAt = DateTime.Now;
            _context.Add(noteShare);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        
        await SetupCreateViewData(userId);
        return View(noteShare);
    }

    // GET: NoteShares/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var noteShare = await _context.NoteShares
            .Include(ns => ns.Note)
            .FirstOrDefaultAsync(ns => ns.NoteShareId == id && ns.Note.UserFK == userId);
            
        if (noteShare == null)
        {
            return NotFound();
        }

        // Only allow editing CanEdit permission
        return View();
    }

    // POST: NoteShares/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("NoteShareId,CanEdit")] NoteShare noteShare)
    {
        if (id != noteShare.NoteShareId)
        {
            return NotFound();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var existingShare = await _context.NoteShares
            .Include(ns => ns.Note)
            .FirstOrDefaultAsync(ns => ns.NoteShareId == id && ns.Note.UserFK == userId);
            
        if (existingShare == null)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                // Only update the CanEdit property
                existingShare.CanEdit = noteShare.CanEdit;
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NoteShareExists(noteShare.NoteShareId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        
        return View();
    }

    // GET: NoteShares/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var noteShare = await _context.NoteShares
            .Include(n => n.Note)
            .Include(n => n.SharedWithUser)
            .FirstOrDefaultAsync(m => m.NoteShareId == id && 
                                     (m.Note.UserFK == userId || m.UserShareFK == userId));
        
        if (noteShare == null)
        {
            return NotFound();
        }

        return View(noteShare);
    }

    // POST: NoteShares/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var noteShare = await _context.NoteShares
            .Include(ns => ns.Note)
            .FirstOrDefaultAsync(ns => ns.NoteShareId == id && 
                                      (ns.Note.UserFK == userId || ns.UserShareFK == userId));
        
        if (noteShare != null)
        {
            _context.NoteShares.Remove(noteShare);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task SetupCreateViewData(string userId)
    {
        var userNotes = await _context.Notes
            .Where(n => n.UserFK == userId && !n.IsDeleted)
            .ToListAsync();
            
        ViewData["NoteShareFK"] = new SelectList(userNotes, "NoteId", "Title");
        ViewData["UserShareFK"] = new SelectList(_context.Users, "Id", "UserName");
    }

    private bool NoteShareExists(int id)
    {
        return _context.NoteShares.Any(e => e.NoteShareId == id);
    }
}