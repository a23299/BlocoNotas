using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlocoNotas.Data;
using BlocoNotas.Models;

namespace BlocoNotas.Controllers.MVC;

/// <summary>
/// Controlador MVC para manipulação das Tags.
/// </summary>
public class TagsController : Controller
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Construtor do controlador TagsController.
    /// </summary>
    /// <param name="context">Contexto do banco de dados</param>
    public TagsController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// GET: Tags
    /// Retorna a lista de todas as tags.
    /// </summary>
    /// <returns>View com a lista de tags</returns>
    public async Task<IActionResult> Index()
    {
        return View(await _context.Tags.ToListAsync());
    }

    /// <summary>
    /// GET: Tags/Details/5
    /// Exibe detalhes da tag especificada pelo id.
    /// </summary>
    /// <param name="id">Id da tag</param>
    /// <returns>View com detalhes da tag ou NotFound se não existir</returns>
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var tag = await _context.Tags
            .FirstOrDefaultAsync(m => m.TagId == id);
        if (tag == null)
        {
            return NotFound();
        }

        return View(tag);
    }

    /// <summary>
    /// GET: Tags/Create
    /// Exibe a página para criar uma nova tag.
    /// </summary>
    /// <returns>View de criação de tag</returns>
    public IActionResult Create()
    {
        return View();
    }

    /// <summary>
    /// POST: Tags/Create
    /// Cria uma nova tag com os dados recebidos.
    /// </summary>
    /// <param name="tag">Objeto Tag recebido do formulário</param>
    /// <returns>Redireciona para Index em caso de sucesso ou retorna a view com erros</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("TagId,Name")] Tag tag)
    {
        if (ModelState.IsValid)
        {
            _context.Add(tag);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(tag);
    }

    /// <summary>
    /// GET: Tags/Edit/5
    /// Exibe o formulário para edição da tag especificada pelo id.
    /// </summary>
    /// <param name="id">Id da tag</param>
    /// <returns>View de edição ou NotFound se não existir</returns>
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var tag = await _context.Tags.FindAsync(id);
        if (tag == null)
        {
            return NotFound();
        }
        return View(tag);
    }

    /// <summary>
    /// POST: Tags/Edit/5
    /// Atualiza a tag com os dados recebidos.
    /// </summary>
    /// <param name="id">Id da tag a ser editada</param>
    /// <param name="tag">Objeto Tag com os dados atualizados</param>
    /// <returns>Redireciona para Index em caso de sucesso ou retorna a view com erros</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("TagId,Name")] Tag tag)
    {
        if (id != tag.TagId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(tag);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TagExists(tag.TagId))
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
        return View(tag);
    }

    /// <summary>
    /// GET: Tags/Delete/5
    /// Exibe confirmação para exclusão da tag especificada pelo id.
    /// </summary>
    /// <param name="id">Id da tag</param>
    /// <returns>View de confirmação ou NotFound se não existir</returns>
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var tag = await _context.Tags
            .FirstOrDefaultAsync(m => m.TagId == id);
        if (tag == null)
        {
            return NotFound();
        }

        return View(tag);
    }

    /// <summary>
    /// POST: Tags/Delete/5
    /// Confirma a exclusão da tag especificada pelo id.
    /// </summary>
    /// <param name="id">Id da tag</param>
    /// <returns>Redireciona para Index após exclusão</returns>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var tag = await _context.Tags.FindAsync(id);
        if (tag != null)
        {
            _context.Tags.Remove(tag);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Verifica se uma tag existe pelo id.
    /// </summary>
    /// <param name="id">Id da tag</param>
    /// <returns>True se a tag existir, False caso contrário</returns>
    private bool TagExists(int id)
    {
        return _context.Tags.Any(e => e.TagId == id);
    }
}
