using Microsoft.AspNetCore.SignalR;

namespace ApiPetFoundation.Notifications.Api.Hubs;

public class NotificationHub : Hub
{
    public async Task NotifyNewPet(object petData)
    {
        await Clients.All.SendAsync("NewPetAvailable", petData);
    }

    public async Task NotifyNewAdoptionRequest(object adoptionData)
    {
        await Clients.All.SendAsync("NewAdoptionRequest", adoptionData);
    }

    public async Task NotifyAdoptionStatusChange(string userId, object statusData)
    {
        await Clients.User(userId).SendAsync("AdoptionStatusChanged", statusData);
    }

    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.SendAsync("Connected", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}
