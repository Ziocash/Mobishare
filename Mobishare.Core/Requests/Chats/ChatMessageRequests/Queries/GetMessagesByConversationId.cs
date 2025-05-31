using System;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mobishare.Core.Data;
using Mobishare.Core.Models.Chats;

namespace Mobishare.Core.Requests.Chats.ChatMessageRequests.Queries;

public class GetMessagesByConversationId : IRequest<IReadOnlyList<ChatMessage>>
{
    public int ConversationId { get; set; }

    public GetMessagesByConversationId(int conversationId)
    {
        ConversationId = conversationId;
    }
}

public class GetMessagesByConversationIdHandler : IRequestHandler<GetMessagesByConversationId, IReadOnlyList<ChatMessage>>
{
    private readonly ApplicationDbContext _dbContext;

    private readonly ILogger<GetMessagesByConversationIdHandler> _logger;

    public GetMessagesByConversationIdHandler(ApplicationDbContext dbContext, ILogger<GetMessagesByConversationIdHandler> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IReadOnlyList<ChatMessage>> Handle(GetMessagesByConversationId request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Retrieving messages for conversation ID {ConversationId}", request.ConversationId);

        if (request.ConversationId <= 0)
        {
            _logger.LogWarning("Invalid conversation ID: {ConversationId}", request.ConversationId);
            return Array.Empty<ChatMessage>();
        }

        var messages = await _dbContext.ChatMessages
            .Where(m => m.ConversationId == request.ConversationId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Found {Count} messages for conversation ID {ConversationId}", messages.Count, request.ConversationId);

        return messages;
    }
}
