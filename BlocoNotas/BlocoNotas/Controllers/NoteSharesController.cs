using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BlocoNotas.Data;
using BlocoNotas.Models;

namespace BlocoNotas.Controllers
{
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
            var applicationDbContext = _context.NoteShares.Include(n => n.Note).Include(n => n.SharedWithUser);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: NoteShares/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var noteShare = await _context.NoteShares
                .Include(n => n.Note)
                .Include(n => n.SharedWithUser)
                .FirstOrDefaultAsync(m => m.NoteShareId == id);
            if (noteShare == null)
            {
                return NotFound();
            }

            return View(noteShare);
        }

        // GET: NoteShares/Create
        public IActionResult Create()
        {
            ViewData["NoteId"] = new SelectList(_context.Notes, "NoteId", "Title");
            ViewData["SharedWithUserId"] = new SelectList(_context.Users, "UserId", "Password");
            return View();
        }

        // POST: NoteShares/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NoteShareId,NoteId,SharedWithUserId,SharedAt,CanEdit")] NoteShare noteShare)
        {
            if (ModelState.IsValid)
            {
                _context.Add(noteShare);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["NoteId"] = new SelectList(_context.Notes, "NoteId", "Title", noteShare.NoteId);
            ViewData["SharedWithUserId"] = new SelectList(_context.Users, "UserId", "Password", noteShare.SharedWithUserId);
            return View(noteShare);
        }

        // GET: NoteShares/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var noteShare = await _context.NoteShares.FindAsync(id);
            if (noteShare == null)
            {
                return NotFound();
            }
            ViewData["NoteId"] = new SelectList(_context.Notes, "NoteId", "Title", noteShare.NoteId);
            ViewData["SharedWithUserId"] = new SelectList(_context.Users, "UserId", "Password", noteShare.SharedWithUserId);
            return View(noteShare);
        }

        // POST: NoteShares/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("NoteShareId,NoteId,SharedWithUserId,SharedAt,CanEdit")] NoteShare noteShare)
        {
            if (id != noteShare.NoteShareId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(noteShare);
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
            ViewData["NoteId"] = new SelectList(_context.Notes, "NoteId", "Title", noteShare.NoteId);
            ViewData["SharedWithUserId"] = new SelectList(_context.Users, "UserId", "Password", noteShare.SharedWithUserId);
            return View(noteShare);
        }

        // GET: NoteShares/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var noteShare = await _context.NoteShares
                .Include(n => n.Note)
                .Include(n => n.SharedWithUser)
                .FirstOrDefaultAsync(m => m.NoteShareId == id);
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
            var noteShare = await _context.NoteShares.FindAsync(id);
            if (noteShare != null)
            {
                _context.NoteShares.Remove(noteShare);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool NoteShareExists(int id)
        {
            return _context.NoteShares.Any(e => e.NoteShareId == id);
        }
    }
}
