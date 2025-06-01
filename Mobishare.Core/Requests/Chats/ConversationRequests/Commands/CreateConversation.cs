using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Mobishare.Core.Data;
using Mobishare.Core.Models.Chats;

namespace Mobishare.Core.Requests.Chats.ConversationRequests.Commands;

public class CreateConversation : Conversation, IRequest<Conversation> { }

public class CreateConversationHandler : IRequestHandler<CreateConversation, Conversation>
{
    private readonly IMapper _mapper;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<CreateConversationHandler> _logger;

    public CreateConversationHandler(IMapper mapper, ApplicationDbContext dbContext, ILogger<CreateConversationHandler> logger)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public async Task<Conversation> Handle(CreateConversation request, CancellationToken cancellationToken)
    {
        var newConversation = _mapper.Map<Conversation>(request);

        try
        {
            _logger.LogDebug("Executing {method}", nameof(CreateConversation));
            _dbContext.Conversations.Entry(newConversation).State = Microsoft.EntityFrameworkCore.EntityState.Added;
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Conversation {conversation} created successfully", newConversation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating conversation");
        }

        return newConversation;
    }
}