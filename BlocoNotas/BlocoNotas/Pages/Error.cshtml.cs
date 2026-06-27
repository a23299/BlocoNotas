using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace BlocoNotas.Pages
{
    /// <summary>
    /// PageModel para exibir erros da aplicação.
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    public class ErrorModel : PageModel
    {
        /// <summary>
        /// ID do pedido para rastreio de erros.
        /// </summary>
        public string? RequestId { get; set; }

        /// <summary>
        /// Indica se o ID do pedido deve ser exibido.
        /// </summary>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        /// <summary>
        /// Endpoint GET. Captura o identificador do pedido atual.
        /// </summary>
        public void OnGet()
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        }
    }
}
