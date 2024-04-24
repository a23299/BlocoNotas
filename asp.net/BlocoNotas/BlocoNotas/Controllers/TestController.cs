using BlocoNotas.Data;
using BlocoNotas.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BlocoNotas.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        public ApplicationDbContext _context;

        public TestController(ApplicationDbContext  context) 
        {
            _context = context;
        }

        [HttpGet]
        [Route("")]
        public ActionResult Index()
        {
            var listaNotasDB = _context.Note.ToList();
            var listaNotasResult = new List<NoteDTO>();

            foreach (var note in listaNotasDB)
            {
                NoteDTO noteDTO = new NoteDTO();
                noteDTO.Titulo = note.Titulo;
                noteDTO.Conteudo = note.Conteudo;

                listaNotasResult.Add(noteDTO);
            }

            return Ok(listaNotasResult);
        }
    }
}
