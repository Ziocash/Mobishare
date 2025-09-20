namespace Mobishare.Ai.ChatBotAIService.ToolExecutor.Tools;

public static class UserContext
{
    private static string _UserId = "";
    private static string _Lat = "";
    private static string _Lon = "";

    public static string UserId
    {
        get => _UserId;
        set => _UserId = value;
    }

    public static string Lat
    {
        get => _Lat;
        set => _Lat = value;
    }

    public static string Lon
    {
        get => _Lon;
        set => _Lon = value;
    }
}