using System;

namespace Mobishare.Infrastructure.Services.ChatBotAIService;

public interface IOllamaService
{
    IAsyncEnumerable<string> StreamResponseAsync(int conversationId, string prompt);
    Task<string> GetResponseAsync(string prompt);
}


