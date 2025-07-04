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
using Mobishare.Infrastructure.Services.ChatBotAIService.IntentClassifier;
using Mobishare.Infrastructure.Services.ChatBotAIService.IntentRouter;
using Mobishare.Core.Services.UserContext;
using Mobishare.Core.Requests.Chats.ChatMessageRequests.Queries;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Text;
using Mobishare.Infrastructure.Services;
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
    private readonly Kernel _kernel;
    private readonly SemanticRouterService _router;
    private static readonly Dictionary<int, Timer> _inactivityTimers = new();
    private static readonly TimeSpan InactivityLimit = TimeSpan.FromMinutes(30);
    private readonly IChatCompletionService _chatService;

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
        IIntentClassificationService intentClassification,
        IIntentRouterService intentRouter,
        IUserContextService userContext,
        Kernel kernel,
        SemanticRouterService router
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
        _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
        _router = router ?? throw new ArgumentNullException(nameof(router));
        _client = new OllamaApiClient(_configuration["Ollama:Llm:UrlApiClient"]!);
        _client.SelectedModel = _configuration["Ollama:Llm:ModelName"]!;
    }

    public async Task SendMessage(string conversationId, string message)
    {
        if (Context.UserIdentifier == null)
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

        // Salvataggio messaggio utente
        var sanitizer = new HtmlSanitizer();
        message = sanitizer.Sanitize(message);
        await Clients.Caller.SendAsync("ReceiveMessage", "User", message, DateTime.UtcNow.ToLocalTime().ToString("g"));

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

        // Preparazione contesto conversazione
        using var scope = _scopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var messages = await mediator.Send(new GetMessagesByConversationId(int.Parse(conversationId)));
        var lastMessages = messages.Take(10);

        try
        {
            var chatHistory = new ChatHistory();
        
        // Aggiungi contesto conversazione
        foreach (var msg in lastMessages)
        {
            var role = msg.Sender == MessageSenderType.User.ToString() 
                ? AuthorRole.User 
                : AuthorRole.Assistant;
            chatHistory.Add(new ChatMessageContent(role, msg.Message));
        }

        chatHistory.AddUserMessage(message);

        var settings = new OpenAIPromptExecutionSettings
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
            Temperature = 0.3
        };

        var completeResponse = new StringBuilder();

        await foreach (var chunk in _chatService.GetStreamingChatMessageContentsAsync(
            chatHistory, 
            settings, 
            _kernel))
        {
            if (chunk.Content is not null)
            {
                completeResponse.Append(chunk.Content);
                await Clients.Caller.SendAsync("ReceiveMessage", 
                    "MobishareBot", 
                    chunk.Content, 
                    DateTime.UtcNow.ToLocalTime().ToString("g"));
            }
        }

        // Aggiungi log per debugging
        _logger.LogInformation("Inizio routing per: {Message}", message);
        
        var route = await _router.RouteFromUserInputAsync(message);
        
        _logger.LogInformation("Risultato routing: {Route} per: {Message}", route, message);

        if (route != "/home")
        {
            await Clients.Caller.SendAsync("RedirectTo", route);
            return;
        }

            // 3. Salvataggio risposta AI
            var aiResponseContent = completeResponse.ToString();
            if (!string.IsNullOrEmpty(aiResponseContent))
            {
                var aiEmbedResponse = await _embeddingService.CreateEmbeddingAsync(aiResponseContent);
                var deserializeAiEmbedResponse = JsonSerializer.Serialize(aiEmbedResponse);

                var aiResponse = await _mediatr.Send(new CreateChatMessage
                {
                    ConversationId = int.Parse(conversationId),
                    Message = aiResponseContent,
                    Sender = MessageSenderType.AiAgent.ToString(),
                    CreatedAt = DateTime.UtcNow,
                    Embedding = deserializeAiEmbedResponse
                });

                if (userResponse != null && aiResponse != null)
                {
                    await _mediatr.Send(new CreateMessagePair
                    {
                        UserMessageId = userResponse.Id,
                        AiMessageId = aiResponse.Id,
                        IsForRag = false,
                        SourceType = SourceType.Chatbot.ToString(),
                        Answered = true,
                        Language = _configuration["Ollama:Llm:ModelName"] ?? "default"
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Errore interno: {ex.Message}");
            await Clients.Caller.SendAsync("ReceiveMessage", "MobishareBot", "An internal error has occurred.", DateTime.UtcNow.ToLocalTime().ToString("g"));

            var aiResponse = await _mediatr.Send(new CreateChatMessage
            {
                ConversationId = int.Parse(conversationId),
                Message = "An internal error has occurred.",
                Sender = MessageSenderType.AiAgent.ToString(),
                CreatedAt = DateTime.UtcNow
            });
        }

        ResetInactivityTimer(conversationId, Context.ConnectionId);
    }

    private string BuildPromptWithTools(string userMessage, IEnumerable<ChatMessage> history)
    {
        var sb = new StringBuilder();
        sb.AppendLine("You are a virtual assistant for Mobishare, a car sharing service similar to Lime.");
        sb.AppendLine("Available tools (you MUST use if needed):");
        sb.AppendLine("- RouteToPageAsync: Use when user wants to navigate to specific pages (account, wallet, profile, rental, support)");
        sb.AppendLine();
        sb.AppendLine("Answer rules:");
        sb.AppendLine("- Be clear and polite");
        sb.AppendLine("- If you don't know the answer, say: 'I can't answer that question. If you have any other questions, I'm here to help!'");
        sb.AppendLine("- When user asks about account, wallet, profile, rental or support, use RouteToPageAsync tool");
        sb.AppendLine();

        if (history.Any())
        {
            sb.AppendLine("Conversation history:");
            foreach (var msg in history)
            {
                sb.AppendLine($"{msg.Sender}: {msg.Message}");
            }
            sb.AppendLine();
        }

        sb.AppendLine($"User's message: {userMessage}");
        sb.AppendLine("Assistant:");

        return sb.ToString();
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
