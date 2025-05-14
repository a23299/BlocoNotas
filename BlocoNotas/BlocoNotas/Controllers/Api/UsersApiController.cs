using Microsoft.AspNetCore.Mvc;

namespace BlocoNotas.Controllers.Api;

public class UsersApiController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}