using System.Text;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mobishare.Core.Enums.Chat;
using Mobishare.Core.Requests.Chats.ChatMessageRequests.Queries;
using OllamaSharp;
using OllamaSharp.Models.Chat;

namespace Mobishare.Ai.ChatBotAIService
{
    public class OllamaService : IOllamaService
    {
        private readonly OllamaApiClient _client;
        private readonly ILogger<OllamaService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _scopeFactory;

        public OllamaService(ILogger<OllamaService> logger, IConfiguration configuration, IServiceScopeFactory scopeFactory)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));

            _client = new OllamaApiClient(_configuration["Ollama:Llms:DefaultUrlApiClient"]!);
            _client.SelectedModel = _configuration["Ollama:Llms:Qwen3:ModelName"]!;
        }

        public async IAsyncEnumerable<string> StreamResponseAsync(int conversationId, string prompt)
        {
            using var scope = _scopeFactory.CreateScope();
            var _mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            var messages = await _mediator.Send(new GetMessagesByConversationId(conversationId));
            var orderedMessages = messages.OrderBy(m => m.CreatedAt).ToList();

            // Converti i messaggi al formato Ollama Message
            var chatHistory = orderedMessages.Select(msg => new Message
            {
                Role = msg.Sender.ToLower(), // es: "user" o "aiagent"
                Content = msg.Message
            }).ToList();

            // Aggiungi l'ultimo messaggio dell'utente come prompt
            chatHistory.Add(new Message
            {
                Role = MessageSenderType.User.ToString().ToLower(),
                Content = prompt
            });

            var req = new ChatRequest
            {
                Model = _client.SelectedModel,
                Messages = chatHistory,
            };

            await foreach (var response in _client.ChatAsync(req))
            {
                if (!string.IsNullOrEmpty(response?.Message?.Content))
                {
                    yield return response.Message.Content;
                }
            }
        }
        
        public async Task<string> GetResponseAsync(string prompt)
        {
            // var req = new ChatRequest
            // {
            //     Model = _client.SelectedModel,
            //     Messages = new List<Message>
            //     {
            //         new Message
            //         {
            //             Role = MessageSenderType.User.ToString().ToLower(),
            //             Content = prompt
            //         }
            //     }
            // };

            var request = new ChatRequest
            {
                Model = _client.SelectedModel,
                Messages = new List<Message>
                {
                    new Message
                    {
                        Role = "user",
                        Content = prompt
                    }
                }
            };

            _logger.LogInformation("Request made to LLM: {request}", request.Messages);

            var responseContent = new StringBuilder();

            await foreach (var response in _client.ChatAsync(request))
            {
                if (!string.IsNullOrEmpty(response?.Message?.Content)) responseContent.Append(response.Message.Content);
            }

            return responseContent.ToString();
        }
    }
}
