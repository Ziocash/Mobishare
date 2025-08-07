using System;

namespace Mobishare.Infrastructure.Services.ChatBotAIService.ToolExecutor.Tools;

public static class UserContext
{
    private static readonly AsyncLocal<string> _UserId = new();

    public static string UserId
    {
        get => _UserId.Value;
        set => _UserId.Value = value;
    }
}