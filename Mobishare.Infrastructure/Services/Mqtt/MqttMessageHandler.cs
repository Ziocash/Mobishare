using System;
using System.Text.Json;
using MediatR;
using Mobishare.Core.Requests.Vehicles.PositionRequests.Commands;
using Microsoft.Extensions.Logging;
using Mobishare.Core.Models.Vehicles;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Mobishare.Infrastructure.Services.SignalR;
using System.Numerics;
using Mobishare.Core.VehicleStatus;

namespace Mobishare.Infrastructure.Services.MQTT;

public class MqttMessageHandler : IDisposable
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly ILogger<MqttMessageHandler> _logger;
    private readonly MqttReceiver _receiver;
    private readonly IHubContext<VehicleHub> _hubContext;

    public MqttMessageHandler(IMediator mediator, IMapper mapper, ILogger<MqttMessageHandler> logger, IHubContext<VehicleHub> hubContext)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));

        _receiver = new MqttReceiver("broker.hivemq.com", "arduino/gps");
        _receiver.TopicMessage += OnMessageReceived;
    }

    public async Task StartAsync() => await _receiver.StartAsync();
    public async Task StopAsync() => await _receiver.StopAsync();

    public void Dispose()
    {
        _receiver.TopicMessage -= OnMessageReceived;
        _receiver.Dispose();
    }

    private async void OnMessageReceived(string payload)
    {
        try
        {

            // Esempio: il payload è un JSON del tipo { "latitude": ..., "longitude": ..., "vehicleId": ... }
            var vehiclePosition = JsonSerializer.Deserialize<Position>(payload, options: new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            if (vehiclePosition is null)
            {
                _logger.LogWarning("Failed to deserialize payload: {Payload}", payload);
                return;
            }

            if (vehiclePosition.Latitude == 0 && vehiclePosition.Longitude == 0)
            {
                _logger.LogWarning("Failed to retrieve position: lat: {Latitude}, lon {Longitude}", vehiclePosition.Latitude, vehiclePosition.Longitude);
                return;
            }

            await _mediator.Send(
                _mapper.Map<CreatePosition>(vehiclePosition)
            );
            
            if(vehiclePosition.Vehicle.Status == VehicleStatusType.Free.ToString())
                await _hubContext.Clients.All.SendAsync("ReceiveVehiclePositionUpdate", vehiclePosition);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing MQTT message: {Payload}", payload);
        }
    }
}
