using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Mobishare.Infrastructure.Services.ChatBotAIService;

public class EmbeddingService : IEmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EmbeddingService> _logger;
    private readonly string _modelName;

    public EmbeddingService(ILogger<EmbeddingService> logger, IConfiguration configuration)
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(configuration["Ollama:Embedding:UrlApiClient"]!)
        };
        _modelName = configuration["Ollama:Embedding:ModelName"]!;
        _logger = logger;
    }

    public async Task<float[]> CreateEmbeddingAsync(string input)
    {
        var requestBody = new
        {
            model = _modelName,
            prompt = input
        };

        var response = await _httpClient.PostAsJsonAsync("/api/embeddings", requestBody);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(content);

        if (jsonDoc.RootElement.TryGetProperty("embedding", out var embeddingJson))
        {
            var embedding = embeddingJson.EnumerateArray().Select(e => e.GetSingle()).ToArray();
            return embedding;
        }

        throw new Exception("Embedding non trovato nella risposta.");
    }
}
