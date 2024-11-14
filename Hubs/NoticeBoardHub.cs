using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace auth_app_backend.Hubs
{
    public class NoticeBoardHub : Hub
    {
        public async Task SendMessage(string sender, string receiver, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", sender, receiver, message, DateTime.UtcNow);
        }
    }
}