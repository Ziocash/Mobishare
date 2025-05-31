using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Mobishare.Core.Enums.Chat;
using OllamaSharp;
using OllamaSharp.Models.Chat;

namespace Mobishare.Infrastructure.Services.ChatBotAIService
{
    public class OllamaService : IOllamaService
    {
        private readonly OllamaApiClient _client;
        private readonly ILogger<OllamaService> _logger;
        private readonly IConfiguration _configuration;

        public OllamaService(ILogger<OllamaService> logger, IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _client = new OllamaApiClient(_configuration["Ollama:UrlApiClient"]!);
            _client.SelectedModel = _configuration["Ollama:ModelName"]!;
        }

        public async IAsyncEnumerable<string> StreamResponseAsync(int conversationId, string prompt)
        {
            // TODO: prendersi tutti i messaggi della conversazione e metterli nella lista dei messaggi
            var req = new ChatRequest
            {
                Model = _client.SelectedModel,
                Messages = new List<Message>
                {
                    new Message
                    {
                        Role = MessageSenderType.User.ToString().ToLower(),
                        Content = prompt
                    }
                },
            };

            await foreach (var response in _client.ChatAsync(req))
            {
                if (!string.IsNullOrEmpty(response?.Message?.Content))
                {
                    yield return response.Message.Content;
                }
            }
        }
    }
}
