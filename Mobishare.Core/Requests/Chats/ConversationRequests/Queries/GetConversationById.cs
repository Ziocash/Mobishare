using System;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mobishare.Core.Data;
using Mobishare.Core.Models.Chats;

namespace Mobishare.Core.Requests.Chats.ConversationRequests.Queries;

public class GetConversationById : IRequest<Conversation>
{
    public int ConversationId { get; set; }

    public GetConversationById(int conversationId)
    {
        if (conversationId <= 0)
            throw new ArgumentOutOfRangeException(nameof(conversationId), "ConversationId must be greater than zero.");
        ConversationId = conversationId;
    }
}

public class GetConversationByIdHandler : IRequestHandler<GetConversationById, Conversation>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<GetConversationByIdHandler> _logger;

    public GetConversationByIdHandler(ApplicationDbContext dbContext, ILogger<GetConversationByIdHandler> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Conversation?> Handle(GetConversationById request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Retrieving conversation for conversation ID {ConversationId}", request.ConversationId);

        if (request.ConversationId <= 0)
        {
            _logger.LogWarning("Conversation ID must be greater than zero.");
            return null;
        }

        var conversation = await _dbContext.Conversations
            .Where(c => c.Id == request.ConversationId)
            .FirstOrDefaultAsync(cancellationToken);

        if (conversation == null)
        {
            _logger.LogWarning("Conversation with ID {ConversationId} not found", request.ConversationId);
            return null;
        }

        _logger.LogInformation("Found conversation with ID {ConversationId}", request.ConversationId);
        return conversation;
    }
}
