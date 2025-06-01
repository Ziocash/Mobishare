using System;
using Mobishare.Core.Models.Chats;

namespace Mobishare.Infrastructure.Services.ChatBotAIService;

public interface IKnowledgeBaseRetriever
{
    /// <summary>
    /// Retrieves the most relevant message pairs from the knowledge base based on the input query.
    /// </summary>
    /// <param name="input">The input query to search for relevant message pairs.</param>
    /// <param name="topN">The number of top relevant pairs to return.</param>
    Task<List<ChatMessage>> GetRelevantPairsAsync(float[] input, int topN = 3);
}