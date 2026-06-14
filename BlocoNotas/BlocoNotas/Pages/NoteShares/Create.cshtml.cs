using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using BlocoNotas.Data;
using BlocoNotas.Hubs;
using BlocoNotas.Models;

namespace BlocoNotas.Pages.NoteShares
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NoteHub> _hubContext;

        public CreateModel(ApplicationDbContext context, IHubContext<NoteHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [BindProperty]
        public NoteShare NoteShare { get; set; } = new();

        public SelectList? NoteList { get; set; }
        public SelectList? UserList { get; set; }

        public async Task OnGetAsync(int? noteId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var userNotes = await _context.Notes
                .Where(n => n.UserFK == userId && !n.IsDeleted)
                .ToListAsync();

            NoteList = new SelectList(userNotes, "NoteId", "Title", noteId);
            UserList = new SelectList(_context.Users.Where(u => u.Id != userId), "Id", "UserName");

            if (noteId.HasValue)
                NoteShare.NoteId = noteId.Value;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var note = await _context.Notes
                .FirstOrDefaultAsync(n => n.NoteId == NoteShare.NoteId && n.UserFK == userId && !n.IsDeleted);

            if (note == null)
            {
                ModelState.AddModelError("", "Nota não encontrada ou não pertence a si.");
                await SetupViewData(userId);
                return Page();
            }

            if (NoteShare.UserShareFK == userId)
            {
                ModelState.AddModelError("", "Não pode partilhar uma nota consigo mesmo.");
                await SetupViewData(userId);
                return Page();
            }

            var existingShare = await _context.NoteShares
                .AnyAsync(ns => ns.NoteId == NoteShare.NoteId && ns.UserShareFK == NoteShare.UserShareFK);

            if (existingShare)
            {
                ModelState.AddModelError("", "Esta nota já está partilhada com este utilizador.");
                await SetupViewData(userId);
                return Page();
            }

            if (!ModelState.IsValid)
            {
                await SetupViewData(userId);
                return Page();
            }

            NoteShare.SharedAt = DateTime.Now;
            _context.NoteShares.Add(NoteShare);
            await _context.SaveChangesAsync();

            var currentUserName = User.Identity?.Name ?? "Alguém";
            var message = $"Nota '{note.Title}' foi partilhada consigo por {currentUserName}.";
            await _hubContext.Clients.User(NoteShare.UserShareFK).SendAsync("ReceiveNotification", message);

            return RedirectToPage("./Index");
        }

        private async Task SetupViewData(string userId)
        {
            var userNotes = await _context.Notes
                .Where(n => n.UserFK == userId && !n.IsDeleted)
                .ToListAsync();

            NoteList = new SelectList(userNotes, "NoteId", "Title", NoteShare.NoteId);
            UserList = new SelectList(_context.Users.Where(u => u.Id != userId), "Id", "UserName");
        }
    }
}
