using System;
using Mobishare.Core.Enums.Ai;

namespace Mobishare.Infrastructure.Services.ChatBotAIService.ToolExecutor;

public interface IToolExecutionService
{
    Task<ToolResult> ExecuteToolAsync(string intent, string message);
}
