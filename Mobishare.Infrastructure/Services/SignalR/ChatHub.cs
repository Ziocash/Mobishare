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
using Mobishare.Core.Requests.Chats.ConversationRequests.Queries;
using Mobishare.Core.Requests.Chats.MessagePairRequests.Commands;
using Mobishare.Infrastructure.Services.ChatBotAIService;
using OllamaSharp;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Mobishare.Core.Requests.Chats.ConversationRequests.Commands;

public class ChatHub : Hub
{
    private readonly IOllamaService _ollamaService;
    private readonly IConfiguration _configuration;
    private readonly IMediator _mediatr;
    private readonly IMapper _mapper;
    private readonly ILogger<ChatHub> _logger;
    private readonly IEmbeddingService _embeddingService;
    private readonly IKnowledgeBaseRetriever _knowledgeBaseRetriever;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHubContext<ChatHub> _hubContext;
    private static readonly Dictionary<int, Timer> _inactivityTimers = new();
    private static readonly TimeSpan InactivityLimit = TimeSpan.FromMinutes(30);

    public ChatHub
    (
        IOllamaService ollamaService,
        IMediator mediator, IMapper mapper,
        ILogger<ChatHub> logger,
        IConfiguration configuration,
        IEmbeddingService embeddingService,
        IKnowledgeBaseRetriever knowledgeBaseRetriever,
        IServiceScopeFactory scopeFactory,
        IHubContext<ChatHub> hubContext
    )
    {
        _knowledgeBaseRetriever = knowledgeBaseRetriever ?? throw new ArgumentNullException(nameof(knowledgeBaseRetriever));
        _embeddingService = embeddingService ?? throw new ArgumentNullException(nameof(embeddingService));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _ollamaService = ollamaService ?? throw new ArgumentNullException(nameof(ollamaService));
        _mediatr = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
    }

    public async Task SendMessage(string conversationId, string message)
    {
        if(Context.UserIdentifier == null)
        {
            _logger.LogWarning("User identifier is null. Cannot process message.");
            await Clients.Caller.SendAsync("ReceiveMessage", "MobishareBot", "You must be logged in to send messages.", DateTime.UtcNow.ToLocalTime().ToString("g"));
            return;
        }

        var userId = Context.UserIdentifier;
        var conversations = await _mediatr.Send(new GetAllConversationsByUserId(userId));

        if (conversations == null || !conversations.Any())
        {
            _logger.LogWarning("No conversations found for user ID {UserId}. Creating a new conversation.", userId);
            var newConversation = await _mediatr.Send(new CreateConversation
            {
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UserId = userId
            });
            conversationId = newConversation.Id.ToString();
        }
        else
        {
            var activeConversations = false;
            var currentConversation = null as Conversation;

            foreach (var conversation in conversations)
            {
                if (conversation.IsActive)
                {
                    activeConversations = true;
                    currentConversation = conversation;
                    break;
                }
            }

            if (!activeConversations)
            {
                var newConversation = await _mediatr.Send(new CreateConversation
                {
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UserId = userId
                });

                conversationId = newConversation.Id.ToString();
            }
            else if (currentConversation != null && currentConversation.Id.ToString() != conversationId)
            {
                _logger.LogInformation($"Switching to existing conversation {currentConversation.Id}");
                conversationId = currentConversation.Id.ToString();
            }
        }

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

        ResetInactivityTimer(conversationId, Context.ConnectionId);
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

    private string InactivityPrompt()
    {
        return $"Translate the following sentence into the language used in the current conversation: The conversation has been automatically closed due to inactivity. If you need assistance, just send a new message to reopen it. If you don't have the context, write the sentence: The conversation has been automatically closed due to inactivity.";
    }

    private void ResetInactivityTimer(string conversationId, string connectionId)
    {
        int parsedConversationId = int.Parse(conversationId);

        if (_inactivityTimers.TryGetValue(parsedConversationId, out var existingTimer))
        {
            existingTimer.Change(InactivityLimit, Timeout.InfiniteTimeSpan);
        }
        else
        {
            var timer = new Timer(async _ =>
            {
                using var scope = _scopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                var completeResponse = string.Empty;
                var sanitizer = new HtmlSanitizer();

                try
                {
                    _logger.LogInformation($"Close chat {conversationId} for inactivity.");

                    await mediator.Send(new CloseConversationById(parsedConversationId));

                    await foreach (var partialResponse in _ollamaService.StreamResponseAsync(int.Parse(conversationId), InactivityPrompt()))
                    {
                        completeResponse += partialResponse;
                        await _hubContext.Clients.Client(connectionId)
                            .SendAsync("ReceiveMessage", "MobishareBot", partialResponse, DateTime.UtcNow.ToLocalTime().ToString("g"));

                    }

                    completeResponse = sanitizer.Sanitize(completeResponse);

                    var aiEmbedResponse = await _embeddingService.CreateEmbeddingAsync(completeResponse);
                    var deserializeAiEmbedResponse = JsonSerializer.Serialize(aiEmbedResponse);

                    var aiMessage = await mediator.Send(new CreateChatMessage
                    {
                        ConversationId = parsedConversationId,
                        Message = completeResponse,
                        Sender = MessageSenderType.AiAgent.ToString(),
                        CreatedAt = DateTime.UtcNow,
                        Embedding = deserializeAiEmbedResponse
                    });

                    _logger.LogDebug("Creating MessagePair with aiId={AiId}", aiMessage?.Id);

                    await mediator.Send(new CreateMessagePair
                    {
                        AiMessageId = aiMessage.Id,
                        IsForRag = false,
                        SourceType = SourceType.Chatbot.ToString(),
                        Answered = true,
                        Language = _configuration["Ollama:Llm:ModelName"] ?? "default"
                    });

                    if (_inactivityTimers.TryGetValue(parsedConversationId, out var expiredTimer))
                    {
                        expiredTimer.Dispose();
                        _inactivityTimers.Remove(parsedConversationId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Errore durante la chiusura automatica della conversazione {conversationId}");
                }
            }, null, InactivityLimit, Timeout.InfiniteTimeSpan);

            _inactivityTimers[parsedConversationId] = timer;
        }
    }
}
