using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mobishare.Core.Data;
using Mobishare.Core.Models.Chats;

namespace Mobishare.Core.Requests.Chats.ConversationRequests.Queries;

public class GetAllConversations : IRequest<List<Conversation>> { }

public class GetAllConversationsHandler : IRequestHandler<GetAllConversations, List<Conversation>>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<GetAllConversationsHandler> _logger;

    public GetAllConversationsHandler(ApplicationDbContext dbContext, ILogger<GetAllConversationsHandler> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<Conversation>> Handle(GetAllConversations request, CancellationToken cancellationToken)
    {
         try
        {
            _logger.LogDebug("Executing {method}", nameof(GetAllConversations));
            var conversations = await _dbContext.Conversations.ToListAsync(cancellationToken);
            return conversations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cities");
            return new List<Conversation>();
        }
    }
}