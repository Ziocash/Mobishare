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
using Mobishare.Infrastructure.Services.ChatBotAIService.ToolExecutor.Tools.VehicleTools;
using Mobishare.Core.Services.UserContext;
using Mobishare.Core.Requests.Chats.ChatMessageRequests.Queries;
using Mobishare.Infrastructure.Services.ChatBotAIService.ToolExecutor.Tools;
using Mobishare.Infrastructure.Services.ChatBotAIService.ToolExecutor.Tools.RoutingTools;

public class ChatHub : Hub
{
    private readonly OllamaApiClient _client;
    private readonly IOllamaService _ollamaService;
    private readonly IConfiguration _configuration;
    private readonly IMediator _mediatr;
    private readonly IMapper _mapper;
    private readonly ILogger<ChatHub> _logger;
    private readonly IEmbeddingService _embeddingService;
    private readonly IKnowledgeBaseRetriever _knowledgeBaseRetriever;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly IUserContextService _userContext;
    private readonly Chat _chat;
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
        IHubContext<ChatHub> hubContext,
        IUserContextService userContext
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
        _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));

        _client = new OllamaApiClient(_configuration["Ollama:Llm:UrlApiClient"]!);
        _client.SelectedModel = _configuration["Ollama:Llm:ModelName"]!;
        _chat = new Chat(_client);
    }

    public async Task SendMessage(string conversationId, string message)
    {
        var userId = Context.UserIdentifier;

        if (userId == null)
        {
            _logger.LogWarning("User identifier is null. Cannot process message.");
            await Clients.Caller.SendAsync("ReceiveMessage", "MobishareBot", "You must be logged in to send messages.", DateTime.UtcNow.ToLocalTime().ToString("g"));
            return;
        }

        int newConversationId = await ConversationAssignmentAsync(conversationId, userId);

        if (newConversationId == 0)
        {
            _logger.LogWarning("Failed to assign a conversation ID. Cannot process message.");
            await Clients.Caller.SendAsync("ReceiveMessage", "MobishareBot", "An error occurred while processing your request. Please try again later.", DateTime.UtcNow.ToLocalTime().ToString("g"));
            return;
        }

        ChatMessage aiResponse;
        bool answered = false;
        var sanitizer = new HtmlSanitizer();
        message = sanitizer.Sanitize(message);

        await Clients.Caller.SendAsync("ReceiveMessage", "User", message, DateTime.UtcNow.ToLocalTime().ToString("g"));

        var userEmbedResponse = await _embeddingService.CreateEmbeddingAsync(message);
        var deserializeEmbedResponse = JsonSerializer.Serialize(userEmbedResponse);

        using var scope = _scopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var messages = await mediator.Send(new GetMessagesByConversationId(newConversationId));
        var lastMessages = messages.Take(10);
        var windowMessages = lastMessages.Select(m => $"{m.Sender}: {m.Message} - {m.CreatedAt}").ToList();

        var userResponse = await _mediatr.Send(new CreateChatMessage
        {
            ConversationId = newConversationId,
            Message = message,
            Sender = MessageSenderType.User.ToString(),
            CreatedAt = DateTime.UtcNow,
            Embedding = deserializeEmbedResponse
        });

        var prompt = BuildPrompt(windowMessages, message);

        _logger.LogInformation($"Message recieved: {message} nella conversazione {conversationId}");

        // 1. Invoca Ollama (con tools disponibili) →  
        // 2. Ollama decide:                            √
        //    - Risponde direttamente?                  √
        //    - Oppure chiama un tool?                  √
        // 2.1 Se serve un tool →
        // 2.2 .NET lo esegue (es. prenota veicolo) →
        // 3. Risultato → mandato a Ollama →               
        // 4. Ollama genera risposta finale →           √
        // 5. .NET la mostra all’utente                 √

        UserContext.UserId = userId;

        var tools = new object[] { new ReportIssueTool(), new ReserveVehicleAsyncTool(), new RoutingPageTool() };
        var aiPromtMessage = "";
        
        await foreach (var response in _chat.SendAsync(prompt, tools))
        {
            aiPromtMessage += response;
            if (aiPromtMessage != "")
                await Clients.Caller.SendAsync("ReceiveMessage", "MobishareBot", response, DateTime.UtcNow.ToLocalTime().ToString("g"));
        }

        var aiEmbedResponse = await _embeddingService.CreateEmbeddingAsync(aiPromtMessage);
        var deserializeAiEmbedResponse = JsonSerializer.Serialize(aiEmbedResponse);

        aiResponse = await _mediatr.Send(new CreateChatMessage
        {
            ConversationId = newConversationId,
            Message = aiPromtMessage,
            Sender = MessageSenderType.AiAgent.ToString(),
            CreatedAt = DateTime.UtcNow,
            Embedding = deserializeAiEmbedResponse
        });

        answered = true;

        if (userResponse != null && aiResponse != null)
        {
            var messagePair = await _mediatr.Send(new CreateMessagePair
            {
                UserMessageId = userResponse.Id,
                AiMessageId = aiResponse.Id,
                IsForRag = false,
                SourceType = SourceType.Chatbot.ToString(),
                Answered = answered,
                Language = _configuration["Ollama:Llm:ModelName"] ?? "default"
            });
        }

        ResetInactivityTimer(newConversationId, Context.ConnectionId);

        // Console.WriteLine(aus);

        // var userEmbedResponse = await _embeddingService.CreateEmbeddingAsync(message);
        // var deserializeEmbedResponse = JsonSerializer.Serialize(userEmbedResponse);

        // var userResponse = await _mediatr.Send(new CreateChatMessage
        // {
        //     ConversationId = newConversationId,
        //     Message = message,
        //     Sender = MessageSenderType.User.ToString(),
        //     CreatedAt = DateTime.UtcNow,
        //     Embedding = deserializeEmbedResponse
        // });

        // Console.WriteLine($"Messaggio ricevuto da te: {message}");

        // TODO: da mettere nel prompt
        // var relevantMessages = await _knowledgeBaseRetriever.GetRelevantPairsAsync(userEmbedResponse, 3);

        // try
        // {
        // string prompt = BuildPrompt(message);

        // await Clients.Caller.SendAsync("ReceiveMessage", "User", message, DateTime.UtcNow.ToLocalTime().ToString("g"));
        // var completeResponse = string.Empty;

        // await foreach (var partialResponse in _ollamaService.StreamResponseAsync(newConversationId, prompt))
        // {
        //     completeResponse += partialResponse;
        //     await Clients.Caller.SendAsync("ReceiveMessage", "MobishareBot", partialResponse, DateTime.UtcNow.ToLocalTime().ToString("g"));
        // }

        //completeResponse = sanitizer.Sanitize(completeResponse);

        //var aiEmbedResponse = await _embeddingService.CreateEmbeddingAsync(completeResponse);
        //var deserializeAiEmbedResponse = JsonSerializer.Serialize(aiEmbedResponse);

        //     aiResponse = await _mediatr.Send(new CreateChatMessage
        //     {
        //         ConversationId = newConversationId,
        //         Message = aiPromtMessage,
        //         Sender = MessageSenderType.AiAgent.ToString(),
        //         CreatedAt = DateTime.UtcNow,
        //         Embedding = deserializeAiEmbedResponse
        //     });

        //     answered = true;
        // }
        // catch (Exception ex)
        // {
        //     _logger.LogError($"Errore interno: {ex.Message}");
        //     await Clients.Caller.SendAsync("ReceiveMessage", "MobishareBot", "An internal error has occurred.", DateTime.UtcNow.ToLocalTime().ToString("g"));
        //     aiResponse = await _mediatr.Send(new CreateChatMessage
        //     {
        //         ConversationId = newConversationId,
        //         Message = "An internal error has occurred.",
        //         Sender = MessageSenderType.AiAgent.ToString(),
        //         CreatedAt = DateTime.UtcNow
        //     });
        // }
    }

    private string BuildPrompt(List<string> userMessages, string promptMessage)
    {
        var messages = "";

        foreach (var message in userMessages) messages += $"{message}\n";

        return $@"
                You are a helpful and polite virtual assistant for Mobishare, a green vehicle sharing service.

                Your job is to answer user questions clearly, concisely, and professionally.

                Context:
                - The conversation history is shown below in the format 'Sender: Message - Timestamp'.
                - Use this history to understand the context, but respond only to the latest user message.

                Guidelines:
                - If you are unsure or lack enough information, respond with:
                ""I'm not sure how to answer that. If you have any other questions, I'm here to help!""
                - Do **not** make up information.
                - Do **not** repeat the user's message in your reply.
                - Be direct, clear, and informative.

                Conversation history:
                {messages}

                Latest user message:
                {promptMessage}
            ";
    }

    private string InactivityPrompt()
    {
        return $"Translate the following sentence into the language used in the current conversation: The conversation has been automatically closed due to inactivity. If you need assistance, just send a new message to reopen it. If you don't have the context, write the sentence: The conversation has been automatically closed due to inactivity.";
    }

    private void ResetInactivityTimer(int conversationId, string connectionId)
    {
        if (_inactivityTimers.TryGetValue(conversationId, out var existingTimer))
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

                    await mediator.Send(new CloseConversationById(conversationId));

                    await foreach (var partialResponse in _ollamaService.StreamResponseAsync(conversationId, InactivityPrompt()))
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
                        ConversationId = conversationId,
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

                    if (_inactivityTimers.TryGetValue(conversationId, out var expiredTimer))
                    {
                        expiredTimer.Dispose();
                        _inactivityTimers.Remove(conversationId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Errore durante la chiusura automatica della conversazione {conversationId}");
                }
            }, null, InactivityLimit, Timeout.InfiniteTimeSpan);

            _inactivityTimers[conversationId] = timer;
        }
    }

    private async Task<int> ConversationAssignmentAsync(string conversationId, string userId)
    {
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

        return int.TryParse(conversationId, out var parsedConversationId) ? parsedConversationId : 0;
    }
}
