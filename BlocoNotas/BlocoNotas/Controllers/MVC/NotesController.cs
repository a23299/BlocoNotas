using BlocoNotas.Data;
using BlocoNotas.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BlocoNotas.Controllers.MVC;

public class NotesController : Controller
{
    private readonly ApplicationDbContext _context;

    public NotesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Notes
    public async Task<IActionResult> Index()
    {
        var applicationDbContext = _context.Notes.Include(n => n.User);
        return View(await applicationDbContext.ToListAsync());
    }

    // GET: Notes/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var note = await _context.Notes
            .Include(n => n.User)
            .FirstOrDefaultAsync(m => m.NoteId == id);
        if (note == null)
        {
            return NotFound();
        }

        return View(note);
    }

    // GET: Notes/Create
    public IActionResult Create()
    {
        ViewData["UserId"] = new SelectList(_context.Users, "UserId", "Password");
        return View();
    }

    // POST: Notes/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("NoteId,Title,Content,CreatedAt,UpdatedAt,IsDeleted,UserId")] Note note)
    {
        if (ModelState.IsValid)
        {
            _context.Add(note);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        ViewData["UserId"] = new SelectList(_context.Users, "UserId", "Password", note.UserFK);
        return View(note);
    }

    // GET: Notes/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var note = await _context.Notes.FindAsync(id);
        if (note == null)
        {
            return NotFound();
        }
        ViewData["UserId"] = new SelectList(_context.Users, "UserId", "Password", note.UserFK);
        return View(note);
    }

    // POST: Notes/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("NoteId,Title,Content,CreatedAt,UpdatedAt,IsDeleted,UserId")] Note note)
    {
        if (id != note.NoteId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(note);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NoteExists(note.NoteId))
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
        ViewData["UserId"] = new SelectList(_context.Users, "UserId", "Password", note.UserFK);
        return View(note);
    }

    // GET: Notes/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var note = await _context.Notes
            .Include(n => n.User)
            .FirstOrDefaultAsync(m => m.NoteId == id);
        if (note == null)
        {
            return NotFound();
        }

        return View(note);
    }

    // POST: Notes/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var note = await _context.Notes.FindAsync(id);
        if (note != null)
        {
            _context.Notes.Remove(note);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool NoteExists(int id)
    {
        return _context.Notes.Any(e => e.NoteId == id);
    }
}