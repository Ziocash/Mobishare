using System;
using Mobishare.Core.Enums.Ai;

namespace Mobishare.Ai.ChatBotAIService.ToolExecutor;

public interface IToolExecutionService
{
    Task<ToolResult> ExecuteToolAsync(string intent, string message);
}
