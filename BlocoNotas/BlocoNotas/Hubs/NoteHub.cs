using Microsoft.AspNetCore.SignalR;

namespace BlocoNotas.Hubs
{
    /// <summary>
    /// SignalR hub for sending real-time notifications to users.
    /// </summary>
    public class NoteHub : Hub
    {
        /// <summary>
        /// Sends a notification message to a specific user.
        /// </summary>
        /// <param name="userId">The ID of the target user.</param>
        /// <param name="message">The notification message.</param>
        public async Task NotifyUser(string userId, string message)
        {
            await Clients.User(userId).SendAsync("ReceiveNotification", message);
        }
    }
}
