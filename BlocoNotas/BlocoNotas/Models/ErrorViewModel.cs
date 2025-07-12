namespace BlocoNotas.Models
{
    /// <summary>
    /// Modelo para representar informações de erro exibidas na View de erro.
    /// </summary>
    public class ErrorViewModel
    {
        /// <summary>
        /// Identificador da requisição que causou o erro.
        /// Pode ser nulo se não estiver disponível.
        /// </summary>
        public string? RequestId { get; set; }

        /// <summary>
        /// Indica se o <see cref="RequestId"/> está presente e deve ser exibido.
        /// </summary>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}