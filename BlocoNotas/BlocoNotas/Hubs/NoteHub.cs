using Microsoft.AspNetCore.SignalR;

namespace BlocoNotas.Hubs
{
    public class NoteHub : Hub
    {
        public async Task NotifyUser(string userId, string message)
        {
            await Clients.User(userId).SendAsync("ReceiveNotification", message);
        }
    }
}
