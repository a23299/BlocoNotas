using BlocoNotas.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlocoNotas.Models;

namespace BlocoNotas.Controllers.MVC
{
    /// <summary>
    /// Controller responsável pelo gerenciamento das notas (Notes).
    /// Fornece as ações para criar, ler, atualizar e deletar notas.
    /// </summary>
    public class NotesController : Controller
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Injeção do contexto da base de dados via construtor.
        /// </summary>
        /// <param name="context">Contexto da aplicação para acessar os dados.</param>
        public NotesController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lista todas as notas incluindo informações do usuário relacionado.
        /// </summary>
        /// <returns>View com a lista das notas.</returns>
        public async Task<IActionResult> Index()
        {
            // Recupera todas as notas com o usuário associado para exibir na view.
            var notes = await _context.Notes
                .Include(n => n.User) // Inclui dados do usuário para cada nota
                .ToListAsync();

            return View(notes);
        }

        /// <summary>
        /// Exibe os detalhes de uma nota específica.
        /// </summary>
        /// <param name="id">Id da nota.</param>
        /// <returns>View com os detalhes da nota ou NotFound se não existir.</returns>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            // Busca a nota pelo id incluindo o usuário associado
            var note = await _context.Notes
                .Include(n => n.User)
                .FirstOrDefaultAsync(n => n.NoteId == id);

            if (note == null) return NotFound();

            return View(note);
        }

        /// <summary>
        /// Exibe o formulário para criação de uma nova nota.
        /// </summary>
        /// <returns>View do formulário de criação.</returns>
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Recebe a requisição POST para criar uma nova nota.
        /// </summary>
        /// <param name="note">Objeto da nota enviado pelo formulário.</param>
        /// <returns>Redireciona para Index se sucesso ou exibe o formulário com erros.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Note note)
        {
            if (ModelState.IsValid)
            {
                _context.Add(note);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            // Se o modelo não for válido, retorna para a view com os dados atuais
            return View(note);
        }

        /// <summary>
        /// Exibe o formulário para editar uma nota existente.
        /// </summary>
        /// <param name="id">Id da nota a ser editada.</param>
        /// <returns>View do formulário ou NotFound se não existir.</returns>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var note = await _context.Notes.FindAsync(id);
            if (note == null) return NotFound();

            return View(note);
        }

        /// <summary>
        /// Recebe a requisição POST para atualizar uma nota.
        /// </summary>
        /// <param name="id">Id da nota a atualizar.</param>
        /// <param name="note">Objeto da nota com os dados atualizados.</param>
        /// <returns>Redireciona para Index se sucesso, senão exibe erros ou NotFound.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Note note)
        {
            if (id != note.NoteId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(note);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Verifica se a nota ainda existe para tratar erros de concorrência
                    if (!_context.Notes.Any(e => e.NoteId == id)) return NotFound();
                    else throw; // Caso contrário, relança a exceção
                }
                return RedirectToAction(nameof(Index));
            }
            return View(note);
        }

        /// <summary>
        /// Exibe a confirmação para deletar uma nota.
        /// </summary>
        /// <param name="id">Id da nota a ser deletada.</param>
        /// <returns>View de confirmação ou NotFound se a nota não existir.</returns>
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var note = await _context.Notes
                .Include(n => n.User) // Inclui dados do usuário para mostrar na confirmação
                .FirstOrDefaultAsync(n => n.NoteId == id);

            if (note == null) return NotFound();

            return View(note);
        }

        /// <summary>
        /// Método POST para confirmar a exclusão da nota.
        /// </summary>
        /// <param name="id">Id da nota a ser deletada.</param>
        /// <returns>Redireciona para Index após a exclusão.</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var note = await _context.Notes.FindAsync(id);
            if (note != null)
            {
                _context.Notes.Remove(note);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
