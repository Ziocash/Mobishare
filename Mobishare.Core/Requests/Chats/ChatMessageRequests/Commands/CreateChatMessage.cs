using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Mobishare.Core.Data;
using Mobishare.Core.Models.Chats;

namespace Mobishare.Core.Requests.Chats.ChatMessageRequests.Commands;

public class CreateChatMessage : ChatMessage, IRequest<ChatMessage> { }

public class CreateChatMessageHandler : IRequestHandler<CreateChatMessage, ChatMessage>
{
    private readonly IMapper _mapper;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<CreateChatMessageHandler> _logger;

    public CreateChatMessageHandler(IMapper mapper, ApplicationDbContext dbContext, ILogger<CreateChatMessageHandler> logger)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public async Task<ChatMessage> Handle(CreateChatMessage request, CancellationToken cancellationToken)
    {
        var newChatMessage = _mapper.Map<ChatMessage>(request);

        try
        {
            _logger.LogDebug("Executing {method}", nameof(CreateChatMessage));
            _dbContext.ChatMessages.Entry(newChatMessage).State = Microsoft.EntityFrameworkCore.EntityState.Added;
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("City {city} created successfully", newChatMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating city");
        }

        return newChatMessage;
    }
}