using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlocoNotas.Data;
using BlocoNotas.Models;

namespace BlocoNotas.Controllers.MVC;

/// <summary>
/// Controlador MVC para manipulação dos usuários do sistema.
/// </summary>
public class UsersController : Controller
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Construtor do controlador UsersController.
    /// </summary>
    /// <param name="context">Contexto do banco de dados</param>
    public UsersController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// GET: Users
    /// Lista todos os usuários.
    /// </summary>
    /// <returns>View com a lista de usuários</returns>
    public async Task<IActionResult> Index()
    {
        return View(await _context.Users.ToListAsync());
    }

    /// <summary>
    /// GET: Users/Details/5
    /// Exibe os detalhes de um usuário específico.
    /// </summary>
    /// <param name="id">Id do usuário</param>
    /// <returns>View com os detalhes do usuário ou NotFound se não existir</returns>
    public async Task<IActionResult> Details(string? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(m => m.Id == id);
        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }

    /// <summary>
    /// GET: Users/Create
    /// Exibe o formulário para criação de um novo usuário.
    /// </summary>
    /// <returns>View de criação de usuário</returns>
    public IActionResult Create()
    {
        return View();
    }

    /// <summary>
    /// POST: Users/Create
    /// Cria um novo usuário com os dados fornecidos.
    /// </summary>
    /// <param name="applicationUser">Objeto ApplicationUser com os dados do usuário</param>
    /// <returns>Redireciona para Index em caso de sucesso ou retorna a view com erros</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("UserId,UserName,Password,CreatedAt")] ApplicationUser applicationUser)
    {
        if (ModelState.IsValid)
        {
            _context.Add(applicationUser);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(applicationUser);
    }

    /// <summary>
    /// GET: Users/Edit/5
    /// Exibe o formulário para edição de um usuário específico.
    /// </summary>
    /// <param name="id">Id do usuário</param>
    /// <returns>View de edição ou NotFound se não existir</returns>
    [HttpGet]
    public async Task<IActionResult> Edit(string? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        return View(user);
    }

    /// <summary>
    /// POST: Users/Edit/5
    /// Atualiza os dados de um usuário existente.
    /// </summary>
    /// <param name="id">Id do usuário a ser editado</param>
    /// <param name="applicationUser">Objeto ApplicationUser com os dados atualizados</param>
    /// <returns>Redireciona para Index em caso de sucesso ou retorna a view com erros</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string? id, [Bind("UserId,UserName,Password,CreatedAt")] ApplicationUser applicationUser)
    {
        if (id != applicationUser.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(applicationUser);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(applicationUser.Id))
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
        return View(applicationUser);
    }

    /// <summary>
    /// GET: Users/Delete/5
    /// Exibe confirmação para exclusão do usuário especificado.
    /// </summary>
    /// <param name="id">Id do usuário</param>
    /// <returns>View de confirmação ou NotFound se não existir</returns>
    public async Task<IActionResult> Delete(string? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(m => m.Id == id);
        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }

    /// <summary>
    /// POST: Users/Delete/5
    /// Confirma a exclusão do usuário especificado.
    /// </summary>
    /// <param name="id">Id do usuário</param>
    /// <returns>Redireciona para Index após exclusão</returns>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string? id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            _context.Users.Remove(user);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Verifica se um usuário existe pelo Id.
    /// </summary>
    /// <param name="id">Id do usuário</param>
    /// <returns>True se o usuário existir, False caso contrário</returns>
    private bool UserExists(string id)
    {
        return _context.Users.Any(e => e.Id == id);
    }
}
