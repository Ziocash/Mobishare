using System;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mobishare.Core.Data;
using Mobishare.Core.Models.Chats;

namespace Mobishare.Core.Requests.Chats.MessagePairRequests.Commands;

public class CreateMessagePair : MessagePair, IRequest<MessagePair> { }

public class CreateMessagePairHandler : IRequestHandler<CreateMessagePair, MessagePair>
{
    private readonly IMapper _mapper;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<CreateMessagePairHandler> _logger;

    public CreateMessagePairHandler(IMapper mapper, IServiceScopeFactory serviceScopeFactory, ILogger<CreateMessagePairHandler> logger)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<MessagePair> Handle(CreateMessagePair request, CancellationToken cancellationToken)
    {
        var newMessagePair = _mapper.Map<MessagePair>(request);

        try
        {
            _logger.LogDebug("Executing {method}", nameof(CreateMessagePair));
            using var scope = _serviceScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.MessagePairs.Entry(newMessagePair).State = Microsoft.EntityFrameworkCore.EntityState.Added;
            await dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Message pair created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating message pair");
        }

        return newMessagePair;
    }
}