using System;

namespace Mobishare.Infrastructure.Services.ChatBotAIService.IntentRouter;

public interface IIntentRouterService
{
    string? Route(string intent);
}
