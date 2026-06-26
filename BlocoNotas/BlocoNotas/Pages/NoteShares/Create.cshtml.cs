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
    /// <summary>
    /// PageModel for sharing a note with another user.
    /// </summary>
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NoteHub> _hubContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateModel"/> class.
        /// </summary>
        /// <param name="context">The application database context.</param>
        /// <param name="hubContext">The SignalR hub context for real-time notifications.</param>
        public CreateModel(ApplicationDbContext context, IHubContext<NoteHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        /// <summary>
        /// Gets or sets the note share being created.
        /// </summary>
        [BindProperty]
        public NoteShare NoteShare { get; set; } = new();

        /// <summary>
        /// Gets or sets the select list of user's notes.
        /// </summary>
        public SelectList? NoteList { get; set; }

        /// <summary>
        /// Gets or sets the select list of available users.
        /// </summary>
        public SelectList? UserList { get; set; }

        /// <summary>
        /// Handles the GET request. Loads the note and user lists.
        /// </summary>
        /// <param name="noteId">Optional pre-selected note ID.</param>
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

        /// <summary>
        /// Handles the POST request. Creates the share and sends a SignalR notification.
        /// </summary>
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

        /// <summary>
        /// Reloads the view data for note and user select lists.
        /// </summary>
        /// <param name="userId">The current user ID.</param>
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
