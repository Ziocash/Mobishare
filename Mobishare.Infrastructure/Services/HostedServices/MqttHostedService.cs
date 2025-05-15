using Microsoft.Extensions.Hosting;
using Mobishare.Infrastructure.Services.MQTT;

namespace Mobishare.Infrastructure.Services.HostedServices;

public class MqttHostedService : IHostedService, IDisposable
{
    private readonly MqttMessageHandler _handler;

    public MqttHostedService(MqttMessageHandler handler)
    {
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
    }

    public void Dispose()
    {
        _handler.Dispose();
    }

    public Task StartAsync(CancellationToken cancellationToken) => _handler.StartAsync();
    public Task StopAsync(CancellationToken cancellationToken) => _handler.StopAsync();
}
