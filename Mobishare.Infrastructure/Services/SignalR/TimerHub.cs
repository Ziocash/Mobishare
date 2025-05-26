using Microsoft.AspNetCore.SignalR;

public class TimerHub : Hub
{
    private readonly TimerService _timerService;

    public TimerHub(TimerService timerService)
    {
        _timerService = timerService;
    }

    public async Task StartTimer()
    {
        await _timerService.Start(Context.ConnectionId);
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _timerService.Disconnect(Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }
}
