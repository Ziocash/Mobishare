using System;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mobishare.Core.Data;
using Mobishare.Core.Models.Chats;

namespace Mobishare.Core.Requests.Chats.ConversationRequests.Queries;

public class GetAllConversationsByUserId : IRequest<IEnumerable<Conversation>>
{
    public string UserId { get; set; }

    public GetAllConversationsByUserId(string userId)
    {
        UserId = userId ?? throw new ArgumentNullException(nameof(userId));
    }
}

public class GetAllConversationsByUserIdHandler : IRequestHandler<GetAllConversationsByUserId, IEnumerable<Conversation>>
{
    private readonly ApplicationDbContext _dbContext;

    private readonly ILogger<GetAllConversationsByUserIdHandler> _logger;

    public GetAllConversationsByUserIdHandler(ApplicationDbContext dbContext, ILogger<GetAllConversationsByUserIdHandler> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<Conversation>> Handle(GetAllConversationsByUserId request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Retrieving conversations for user ID {UserId}", request.UserId);

        if (string.IsNullOrEmpty(request.UserId))
        {
            _logger.LogWarning("User ID is null or empty.");
            return Enumerable.Empty<Conversation>();
        }

        var conversations = await _dbContext.Conversations
            .Where(c => c.UserId == request.UserId && c.IsActive)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Found {Count} conversations for user ID {UserId}", conversations.Count, request.UserId);

        return conversations;
    }
}
