using Mobishare.Ai.ChatBotAIService.ToolExecutor;

namespace Mobishare.Ai.ChatBotAIService.IntentRouter;

public interface IIntentRouterService
{
    Task<ToolResult> Route(string intent, string message);
}
