namespace Mobishare.Ai.ChatBotAIService.ToolExecutor.Tools;

public static class UserContext
{
    private static readonly AsyncLocal<string> _UserId = new();
    private static readonly AsyncLocal<string> _Lat = new();
    private static readonly AsyncLocal<string> _Lon = new();

    public static string UserId
    {
        get => _UserId.Value;
        set => _UserId.Value = value;
    }

    public static string Lat
    {
        get => _Lat.Value;
        set => _Lat.Value = value;
    }

    public static string Lon
    {
        get => _Lon.Value;
        set => _Lon.Value = value;
    }
}