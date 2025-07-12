using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BlocoNotas.Data;
using BlocoNotas.Models;

namespace BlocoNotas.Controllers.MVC
{
    /// <summary>
    /// Controller para gerenciar o compartilhamento de notas entre usuários.
    /// </summary>
    public class NoteSharesController : Controller
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Construtor que recebe o contexto da aplicação para acesso ao banco de dados.
        /// </summary>
        /// <param name="context">Contexto da aplicação (Entity Framework).</param>
        public NoteSharesController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Exibe a lista de notas compartilhadas pelo usuário e compartilhadas com o usuário.
        /// </summary>
        /// <returns>View com as listas de compartilhamentos.</returns>
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Recupera as notas compartilhadas pelo usuário e não deletadas
            var sharedByMe = await _context.NoteShares
                .Where(ns => ns.Note.UserFK == userId && !ns.Note.IsDeleted)
                .Include(ns => ns.Note)
                .Include(ns => ns.SharedWithUser)
                .ToListAsync();

            // Recupera os compartilhamentos onde o usuário recebeu notas de outros
            var sharedWithMeNoteShares = await _context.NoteShares
                .Where(ns => ns.UserShareFK == userId && !ns.Note.IsDeleted)
                .Include(ns => ns.Note)
                .ToListAsync();

            var sharedWithMe = new List<Note>();

            // Carrega o dono de cada nota compartilhada comigo
            foreach (var ns in sharedWithMeNoteShares)
            {
                await _context.Entry(ns.Note).Reference(n => n.User).LoadAsync();
                sharedWithMe.Add(ns.Note);
            }

            // Debug na saída do console
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

        /// <summary>
        /// Mostra os detalhes de um compartilhamento específico.
        /// </summary>
        /// <param name="id">ID do compartilhamento.</param>
        /// <returns>View com os detalhes do compartilhamento, ou erro caso não encontrado.</returns>
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

        /// <summary>
        /// Exibe o formulário para criar um novo compartilhamento de nota.
        /// </summary>
        /// <returns>View com o formulário de criação.</returns>
        public async Task<IActionResult> Create()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // Busca notas do usuário que não estejam deletadas
            var userNotes = await _context.Notes
                .Where(n => n.UserFK == userId && !n.IsDeleted)
                .ToListAsync();

            // Popula dropdowns para seleção
            ViewData["NoteShareFK"] = new SelectList(userNotes, "NoteId", "Title");
            ViewData["UserShareFK"] = new SelectList(_context.Users, "Id", "UserName");

            return View();
        }

        /// <summary>
        /// Processa o POST do formulário para criar um novo compartilhamento.
        /// </summary>
        /// <param name="noteShare">Objeto com os dados do compartilhamento.</param>
        /// <returns>Redireciona para Index em sucesso ou mostra erros.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NoteShareFK,UserShareFK,CanEdit")] NoteShare noteShare)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Verifica se a nota pertence ao usuário e não está deletada
            var note = await _context.Notes
                .FirstOrDefaultAsync(n => n.NoteId == noteShare.NoteId && n.UserFK == userId && !n.IsDeleted);

            if (note == null)
            {
                ModelState.AddModelError("", "Nota não encontrada ou não pertence a você.");
                await SetupCreateViewData(userId);
                return View(noteShare);
            }

            // Impede o compartilhamento da nota com o próprio usuário
            if (noteShare.UserShareFK == userId)
            {
                ModelState.AddModelError("", "Não é possível partilhar uma nota consigo mesmo.");
                await SetupCreateViewData(userId);
                return View(noteShare);
            }

            // Verifica se já existe compartilhamento com esse usuário
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

        /// <summary>
        /// Exibe o formulário para editar um compartilhamento específico.
        /// </summary>
        /// <param name="id">ID do compartilhamento.</param>
        /// <returns>View para edição ou NotFound se não encontrado.</returns>
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

            // Apenas permite editar a permissão CanEdit
            return View();
        }

        /// <summary>
        /// Processa o POST para atualizar o compartilhamento.
        /// </summary>
        /// <param name="id">ID do compartilhamento.</param>
        /// <param name="noteShare">Objeto com dados atualizados.</param>
        /// <returns>Redireciona para Index ou mostra erros.</returns>
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
                    // Atualiza somente a permissão CanEdit
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

        /// <summary>
        /// Exibe a confirmação para deletar um compartilhamento.
        /// </summary>
        /// <param name="id">ID do compartilhamento.</param>
        /// <returns>View de confirmação ou NotFound.</returns>
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

        /// <summary>
        /// Processa a exclusão confirmada do compartilhamento.
        /// </summary>
        /// <param name="id">ID do compartilhamento.</param>
        /// <returns>Redireciona para Index.</returns>
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

        /// <summary>
        /// Prepara os dados para exibir nos dropdowns do formulário de criação.
        /// </summary>
        /// <param name="userId">ID do usuário atual.</param>
        /// <returns>Task assíncrona.</returns>
        private async Task SetupCreateViewData(string userId)
        {
            var userNotes = await _context.Notes
                .Where(n => n.UserFK == userId && !n.IsDeleted)
                .ToListAsync();

            ViewData["NoteShareFK"] = new SelectList(userNotes, "NoteId", "Title");
            ViewData["UserShareFK"] = new SelectList(_context.Users, "Id", "UserName");
        }

        /// <summary>
        /// Verifica se um compartilhamento existe no banco.
        /// </summary>
        /// <param name="id">ID do compartilhamento.</param>
        /// <returns>True se existir, falso caso contrário.</returns>
        private bool NoteShareExists(int id)
        {
            return _context.NoteShares.Any(e => e.NoteShareId == id);
        }
    }
}
