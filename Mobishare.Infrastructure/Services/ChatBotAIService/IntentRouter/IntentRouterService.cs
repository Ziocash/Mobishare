using System;
using Mobishare.Core.Enums.Ai;

namespace Mobishare.Infrastructure.Services.ChatBotAIService.IntentRouter;

public class IntentRouterService : IIntentRouterService
{
    // esiste perché posso chiamate intent che sono presenti in altri enum
    // necessito di chiamare dei ToolExecution diversi
    // public async string? Route(string intent)
    // {
    //     if (Enum.TryParse<ToolsClassification>(intent, out var toolIntent))
    //     {
    //         return await ;
    //     }
    //     else
    //     {
    //         return null;
    //     }
    // }
    public string? Route(string intent)
    {
        throw new NotImplementedException();
    }
}
