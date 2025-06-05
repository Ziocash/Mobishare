using System;

namespace Mobishare.Infrastructure.Services.ChatBotAIService.IntentClassifier;

public interface IIntentClassificationService
{
    Task<string> ClassifyMessageAsync(string userMessage);
}
