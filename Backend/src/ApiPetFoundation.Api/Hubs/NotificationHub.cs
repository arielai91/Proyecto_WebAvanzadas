using Microsoft.AspNetCore.SignalR;

namespace ApiPetFoundation.Api.Hubs;

public class NotificationHub : Hub
{
    // Notificar cuando se agrega una nueva mascota
    public async Task NotifyNewPet(object petData)
    {
        await Clients.All.SendAsync("NewPetAvailable", petData);
    }

    // Notificar cuando se crea una solicitud de adopción
    public async Task NotifyNewAdoptionRequest(object adoptionData)
    {
        await Clients.All.SendAsync("NewAdoptionRequest", adoptionData);
    }

    // Notificar cuando cambia el estado de una adopción
    public async Task NotifyAdoptionStatusChange(string userId, object statusData)
    {
        await Clients.User(userId).SendAsync("AdoptionStatusChanged", statusData);
    }

    // Método cuando un cliente se conecta
    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.SendAsync("Connected", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    // Método cuando un cliente se desconecta
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}
