using System;

namespace Mobishare.Infrastructure.Services.ChatBotAIService;

public interface IEmbeddingService
{
    Task<float[]> CreateEmbeddingAsync(string input);
}
