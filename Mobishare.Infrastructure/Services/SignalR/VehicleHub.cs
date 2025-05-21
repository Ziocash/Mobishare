using Microsoft.AspNetCore.SignalR;
using Mobishare.Core.Models.Vehicles;

namespace Mobishare.Infrastructure.Services.SignalR;

public class VehicleHub : Hub
{
    public async Task BroadcastVehicles(List<Vehicle> vehicles)
    {
        await Clients.All.SendAsync("ReceivesVehicles", vehicles);    
    }
    
}
