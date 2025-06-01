using Microsoft.Extensions.Configuration;
using AutoMapper;
using Ganss.Xss;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Mobishare.Core.Enums.Ai;
using Mobishare.Core.Enums.Chat;
using Mobishare.Core.Models.Chats;
using Mobishare.Core.Requests.Chats.ChatMessageRequests.Commands;
using Mobishare.Core.Requests.Chats.MessagePairRequests.Commands;
using Mobishare.Infrastructure.Services.ChatBotAIService;
using OllamaSharp;
using System.Text.Json;

public class ChatHub : Hub
{
    private readonly IOllamaService _ollamaService;
    private readonly IConfiguration _configuration;
    private readonly IMediator _mediatr;
    private readonly IMapper _mapper;
    private readonly ILogger<ChatHub> _logger;
    private readonly IEmbeddingService _embeddingService;
    private readonly IKnowledgeBaseRetriever _knowledgeBaseRetriever;

    public ChatHub
    (
        IOllamaService ollamaService,
        IMediator mediator, IMapper mapper,
        ILogger<ChatHub> logger,
        IConfiguration configuration,
        IEmbeddingService embeddingService,
        IKnowledgeBaseRetriever knowledgeBaseRetriever
    )
    {
        _knowledgeBaseRetriever = knowledgeBaseRetriever ?? throw new ArgumentNullException(nameof(knowledgeBaseRetriever));
        _embeddingService = embeddingService ?? throw new ArgumentNullException(nameof(embeddingService));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _ollamaService = ollamaService ?? throw new ArgumentNullException(nameof(ollamaService));
        _mediatr = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task SendMessage(string conversationId, string message)
    {
        ChatMessage aiResponse;
        bool answered = false;
        var sanitizer = new HtmlSanitizer();
        message = sanitizer.Sanitize(message);

        _logger.LogInformation($"Ricevuto messaggio: {message} nella conversazione {conversationId}");
        var userEmbedResponse = await _embeddingService.CreateEmbeddingAsync(message);
        var deserializeEmbedResponse = JsonSerializer.Serialize(userEmbedResponse);

        var userResponse = await _mediatr.Send(new CreateChatMessage
        {
            ConversationId = int.Parse(conversationId),
            Message = message,
            Sender = MessageSenderType.User.ToString(),
            CreatedAt = DateTime.UtcNow,
            Embedding = deserializeEmbedResponse
        }); 

        Console.WriteLine($"Messaggio ricevuto da te: {message}");

        // TODO: da mettere nel prompt
        var relevantMessages = await _knowledgeBaseRetriever.GetRelevantPairsAsync(userEmbedResponse, 3);

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

            completeResponse = sanitizer.Sanitize(completeResponse);

            var aiEmbedResponse = await _embeddingService.CreateEmbeddingAsync(completeResponse);
            var deserializeAiEmbedResponse = JsonSerializer.Serialize(aiEmbedResponse);

            aiResponse = await _mediatr.Send(new CreateChatMessage
            {
                ConversationId = int.Parse(conversationId),
                Message = completeResponse,
                Sender = MessageSenderType.AiAgent.ToString(),
                CreatedAt = DateTime.UtcNow,
                Embedding = deserializeAiEmbedResponse
            });

            answered = true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Errore interno: {ex.Message}");
            await Clients.Caller.SendAsync("ReceiveMessage", "MobishareBot", "An internal error has occurred.", DateTime.UtcNow.ToLocalTime().ToString("g"));
            aiResponse = await _mediatr.Send(new CreateChatMessage
            {
                ConversationId = int.Parse(conversationId),
                Message = "An internal error has occurred.",
                Sender = MessageSenderType.AiAgent.ToString(),
                CreatedAt = DateTime.UtcNow
            });
        }
        
        if (userResponse != null && aiResponse != null)
        {
            var messagePair = await _mediatr.Send(new CreateMessagePair
            {
                UserMessageId = userResponse.Id,
                AiMessageId = aiResponse.Id,
                IsForRag = true, // TODO: cambiare a FALSE!
                SourceType = SourceType.Chatbot.ToString(),
                Answered = answered,
                Language = _configuration["Ollama:Llm:ModelName"] ?? "default"
            });
        }
    }

    private string BuildPrompt(string userMessage)
    {
        return $@"You are a virtual assistant for Mobishare, a car sharing service similar to Lime.
                Answer questions clearly and politely.
                If you don't know the answer to a question, don't provide inaccurate or made-up information. Instead, politely respond with: 'I can't answer that question. If you have any other questions, I'm here to help!' Only respond when you are sure and have enough information.
                the message below will be the user's message, respond clearly and concisely, without repeating the user's message.

                User's message: {userMessage}
                ";
    }
}
