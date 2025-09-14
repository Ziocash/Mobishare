using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Mobishare.Ai.ChatBotAIService;

/// <summary>
/// Service responsible for creating text embeddings by calling an external API.
/// </summary>
public class EmbeddingService : IEmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EmbeddingService> _logger;
    private readonly string _modelName;

    /// <summary>
    /// Initializes a new instance of <see cref="EmbeddingService"/>.
    /// </summary>
    /// <param name="logger">Logger instance for logging purposes.</param>
    /// <param name="configuration">Application configuration for retrieving API settings.</param>
    public EmbeddingService(ILogger<EmbeddingService> logger, IConfiguration configuration)
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(configuration["Ollama:Embedding:UrlApiClient"]!)
        };
        _modelName = configuration["Ollama:Embedding:ModelName"]!;
        _logger = logger;
    }

    /// <summary>
    /// Asynchronously creates an embedding vector for the provided input text.
    /// </summary>
    /// <param name="input">The input text to generate the embedding for.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that resolves to an array of floats representing the embedding vector.
    /// </returns>
    /// <exception cref="HttpRequestException">Thrown when the HTTP response is unsuccessful.</exception>
    /// <exception cref="Exception">Thrown when the embedding is not found in the API response.</exception>
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

        throw new Exception("Embedding not found in the answer.");
    }
}
