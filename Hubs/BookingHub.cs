using Microsoft.AspNetCore.SignalR;

namespace CoWorkManager.Hubs
{
    public class BookingHub : Hub
    {
    
        public async Task NotifyAll(string message)
        {
            await Clients.All.SendAsync("ShowNotification", message);
        }
    }
}