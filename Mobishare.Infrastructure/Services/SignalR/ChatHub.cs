using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Mobishare.Core.Enums.Chat;
using Mobishare.Core.Models.Chats;
using Mobishare.Core.Requests.Chats.ChatMessageRequests.Commands;
using Mobishare.Infrastructure.Services.ChatBotAIService;
using System.Diagnostics;

public class ChatHub : Hub
{
    private readonly IOllamaService _ollamaService;
    private readonly IMediator _mediatr;
    private readonly IMapper _mapper;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(IOllamaService ollamaService, IMediator mediator, IMapper mapper, ILogger<ChatHub> logger)
    {
        _ollamaService = ollamaService ?? throw new ArgumentNullException(nameof(ollamaService));
        _mediatr = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task SendMessage(string conversationId, string message)
    {
        _logger.LogInformation($"Ricevuto messaggio: {message} nella conversazione {conversationId}");
        var response = await _mediatr.Send(new CreateChatMessage {
            ConversationId = int.Parse(conversationId),
            Message = message,
            Sender = MessageSenderType.User.ToString(),
            CreatedAt = DateTime.UtcNow
        });
        
        Console.WriteLine($"Messaggio ricevuto da te: {message}");

        try
        {
            string prompt = BuildPrompt(message);

            await Clients.Caller.SendAsync("ReceiveMessage", "User", message, DateTime.UtcNow.ToLocalTime().ToString("g"));
            var completeResponse = string.Empty;

            await foreach (var partialResponse in _ollamaService.StreamResponseAsync(int.Parse(conversationId), prompt))
            {
                completeResponse += partialResponse;
                await Clients.Caller.SendAsync("ReceiveMessage", "MobishareBot", partialResponse, DateTime.UtcNow.ToLocalTime().ToString("g"));
            }

            var aiResponse = await _mediatr.Send(new CreateChatMessage
            {
                ConversationId = int.Parse(conversationId),
                Message = completeResponse,
                Sender = MessageSenderType.AiAgent.ToString(),
                CreatedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Errore interno: {ex.Message}");
            await Clients.Caller.SendAsync("ReceiveMessage", "MobishareBot", "An internal error has occurred.", DateTime.UtcNow.ToLocalTime().ToString("g"));
            var aiResponse = await _mediatr.Send(new CreateChatMessage {
                ConversationId = int.Parse(conversationId),
                Message = "An internal error has occurred.",
                Sender = MessageSenderType.AiAgent.ToString(),
                CreatedAt = DateTime.UtcNow
            });
        }
    }

    private string BuildPrompt(string userMessage)
    {
        return $@"Sei un assistente virtuale per Mobishare, servizio di car sharing simile a Lime.
                Rispondi alle domande in modo chiaro e cortese.

                quello che verra scritto subito sotto sarà il messaggio dell'utente, rispondi in modo chiaro e conciso, senza ripetere il messaggio dell'utente.

                messagio dell' Utente: {userMessage}
                ";
    }
}
