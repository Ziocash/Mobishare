using System;

namespace Mobishare.Ai.ChatBotAIService;

public interface IOllamaService
{
    IAsyncEnumerable<string> StreamResponseAsync(int conversationId, string prompt);
    Task<string> GetResponseAsync(string prompt);
}


