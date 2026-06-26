using Microsoft.AspNetCore.SignalR;

namespace BlocoNotas.Hubs
{
    /// <summary>
    /// Hub SignalR para envio de notificações em tempo real para os utilizadores.
    /// </summary>
    public class NoteHub : Hub
    {
        /// <summary>
        /// Envia uma mensagem de notificação para um utilizador específico.
        /// </summary>
        /// <param name="userId">ID do utilizador de destino.</param>
        /// <param name="message">Mensagem de notificação.</param>
        public async Task NotifyUser(string userId, string message)
        {
            await Clients.User(userId).SendAsync("ReceiveNotification", message);
        }
    }
}
