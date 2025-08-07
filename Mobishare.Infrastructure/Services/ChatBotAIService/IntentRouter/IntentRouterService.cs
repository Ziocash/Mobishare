using System;
using Microsoft.Extensions.Logging;
using Mobishare.Core.Enums.Ai;
using Mobishare.Infrastructure.Services.ChatBotAIService.ToolExecutor;

namespace Mobishare.Infrastructure.Services.ChatBotAIService.IntentRouter;

public class IntentRouterService : IIntentRouterService
{
    private readonly IToolExecutionService _toolExecution;
    private readonly ILogger<IntentRouterService> _logger;
    public IntentRouterService(IToolExecutionService toolExecution,  ILogger<IntentRouterService> logger)
    {
        _toolExecution = toolExecution ?? throw new ArgumentNullException(nameof(toolExecution));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // esiste perché posso chiamate intent che sono presenti in altri enum
    // necessito di chiamare dei ToolExecution diversi
    public async Task<ToolResult> Route(string intent, string message)
    {
        if (!Enum.TryParse<ToolsClassification>(intent, out var toolIntent))
            return null;

        return await _toolExecution.ExecuteToolAsync(intent, message);
        
    }
}
