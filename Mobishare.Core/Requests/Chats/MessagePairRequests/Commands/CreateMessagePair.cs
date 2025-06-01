using System;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Mobishare.Core.Data;
using Mobishare.Core.Models.Chats;

namespace Mobishare.Core.Requests.Chats.MessagePairRequests.Commands;

public class CreateMessagePair : MessagePair, IRequest<MessagePair> { }

public class CreateMessagePairHandler : IRequestHandler<CreateMessagePair, MessagePair>
{
    private readonly IMapper _mapper;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<CreateMessagePairHandler> _logger;

    public CreateMessagePairHandler(IMapper mapper, ApplicationDbContext dbContext, ILogger<CreateMessagePairHandler> logger)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<MessagePair> Handle(CreateMessagePair request, CancellationToken cancellationToken)
    {
        var newMessagePair = _mapper.Map<MessagePair>(request);

        try
        {
            _logger.LogDebug("Executing {method}", nameof(CreateMessagePair));
            _dbContext.MessagePairs.Entry(newMessagePair).State = Microsoft.EntityFrameworkCore.EntityState.Added;
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Message pair created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating message pair");
        }

        return newMessagePair;
    }
}