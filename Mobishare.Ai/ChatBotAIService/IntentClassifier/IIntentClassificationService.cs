using System;

namespace Mobishare.Ai.ChatBotAIService.IntentClassifier;

public interface IIntentClassificationService
{
    Task<string> ClassifyMessageAsync(string userMessage);
}
