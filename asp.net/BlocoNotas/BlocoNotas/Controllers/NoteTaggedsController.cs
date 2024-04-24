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
    public class NoteTaggedsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NoteTaggedsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: NoteTaggeds
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.NoteTagged.Include(n => n.Note).Include(n => n.Tag);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: NoteTaggeds/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var noteTagged = await _context.NoteTagged
                .Include(n => n.Note)
                .Include(n => n.Tag)
                .FirstOrDefaultAsync(m => m.NoteTaggedId == id);
            if (noteTagged == null)
            {
                return NotFound();
            }

            return View(noteTagged);
        }

        // GET: NoteTaggeds/Create
        public IActionResult Create()
        {
            ViewData["NoteFK"] = new SelectList(_context.Note, "NoteId", "Titulo");
            ViewData["TagFK"] = new SelectList(_context.Tag, "TagId", "Name");
            return View();
        }

        // POST: NoteTaggeds/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NoteTaggedId,NoteFK,TagFK")] NoteTagged noteTagged)
        {
            if (ModelState.IsValid)
            {
                _context.Add(noteTagged);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["NoteFK"] = new SelectList(_context.Note, "NoteId", "Titulo", noteTagged.NoteFK);
            ViewData["TagFK"] = new SelectList(_context.Tag, "TagId", "Name", noteTagged.TagFK);
            return View(noteTagged);
        }

        // GET: NoteTaggeds/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var noteTagged = await _context.NoteTagged.FindAsync(id);
            if (noteTagged == null)
            {
                return NotFound();
            }
            ViewData["NoteFK"] = new SelectList(_context.Note, "NoteId", "Titulo", noteTagged.NoteFK);
            ViewData["TagFK"] = new SelectList(_context.Tag, "TagId", "Name", noteTagged.TagFK);
            return View(noteTagged);
        }

        // POST: NoteTaggeds/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("NoteTaggedId,NoteFK,TagFK")] NoteTagged noteTagged)
        {
            if (id != noteTagged.NoteTaggedId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(noteTagged);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NoteTaggedExists(noteTagged.NoteTaggedId))
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
            ViewData["NoteFK"] = new SelectList(_context.Note, "NoteId", "Titulo", noteTagged.NoteFK);
            ViewData["TagFK"] = new SelectList(_context.Tag, "TagId", "Name", noteTagged.TagFK);
            return View(noteTagged);
        }

        // GET: NoteTaggeds/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var noteTagged = await _context.NoteTagged
                .Include(n => n.Note)
                .Include(n => n.Tag)
                .FirstOrDefaultAsync(m => m.NoteTaggedId == id);
            if (noteTagged == null)
            {
                return NotFound();
            }

            return View(noteTagged);
        }

        // POST: NoteTaggeds/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var noteTagged = await _context.NoteTagged.FindAsync(id);
            if (noteTagged != null)
            {
                _context.NoteTagged.Remove(noteTagged);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool NoteTaggedExists(int id)
        {
            return _context.NoteTagged.Any(e => e.NoteTaggedId == id);
        }
    }
}
