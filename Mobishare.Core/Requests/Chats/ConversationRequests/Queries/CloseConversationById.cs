using System;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mobishare.Core.Data;
using Mobishare.Core.Models.Chats;

namespace Mobishare.Core.Requests.Chats.ConversationRequests.Queries;

public class CloseConversationById : IRequest<Conversation>
{
    public int ConversationId { get; set; }

    public CloseConversationById(int conversationId)
    {
        if (conversationId <= 0)
            throw new ArgumentOutOfRangeException(nameof(conversationId), "ConversationId must be greater than zero.");
        ConversationId = conversationId;
    }
}

public class CloseConversationByIdHandler : IRequestHandler<CloseConversationById, Conversation>
{
    private readonly ApplicationDbContext _dbContext;

    private readonly ILogger<CloseConversationByIdHandler> _logger;

    public CloseConversationByIdHandler(ApplicationDbContext dbContext, ILogger<CloseConversationByIdHandler> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    public async Task<Conversation?> Handle(CloseConversationById request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Close conversations for conversation ID {ConversationId}", request.ConversationId);

        if (request.ConversationId < 0)
        {
            _logger.LogWarning("Conversation id must greater than zero.");
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

        conversation.IsActive = false;
        _dbContext.Conversations.Update(conversation);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Found conversation with ID {ConversationId}", request.ConversationId);

        return conversation;
    }
}