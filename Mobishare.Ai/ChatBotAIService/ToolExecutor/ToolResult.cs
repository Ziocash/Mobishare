using System;

namespace Mobishare.Ai.ChatBotAIService.ToolExecutor;

public class ToolResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }

    public static ToolResult Ok(string? message = null) =>
        new ToolResult { Success = true, Message = message };

    public static ToolResult Fail(string message) =>
        new ToolResult { Success = false, Message = message };
}
