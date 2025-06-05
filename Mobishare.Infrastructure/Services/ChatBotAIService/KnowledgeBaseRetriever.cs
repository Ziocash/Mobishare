using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Mobishare.Core.Data;
using Mobishare.Core.Models.Chats;


namespace Mobishare.Infrastructure.Services.ChatBotAIService;

public class KnowledgeBaseRetriever : IKnowledgeBaseRetriever
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IEmbeddingService _embedding;

    public KnowledgeBaseRetriever(ApplicationDbContext dbContext, IEmbeddingService embedding)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _embedding = embedding ?? throw new ArgumentNullException(nameof(embedding));
    }

    public async Task<List<ChatMessage>> GetRelevantPairsAsync(float[] input, int topN = 3)
    {        
        var messagePairs = await _dbContext.MessagePairs
            .Include(mp => mp.UserMessage)
            .Include(mp => mp.AiMessage)
            .Where(mp => mp.IsForRag && mp.UserMessage.Embedding != null)
            .ToListAsync();

        return messagePairs
            .Select(mp =>
            {
                var vector = JsonSerializer.Deserialize<float[]>(mp.UserMessage.Embedding)!;
                var sim = CosineSimilarity(input, vector);
                return (mp.UserMessage, mp.AiMessage, sim);
            })
            .OrderByDescending(x => x.sim)
            .Take(topN)
            .Select(x => x.AiMessage)
            .ToList();
    }

    private double CosineSimilarity(float[] a, float[] b)
    {
        double dot = 0, magA = 0, magB = 0;

        for (int i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            magA += a[i] * a[i];
            magB += b[i] * b[i];
        }

        return dot / (Math.Sqrt(magA) * Math.Sqrt(magB));
    }
}
