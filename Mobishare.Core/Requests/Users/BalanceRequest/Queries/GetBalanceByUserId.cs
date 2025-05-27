using System;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mobishare.Core.Data;
using Mobishare.Core.Models.UserRelated;

namespace Mobishare.Core.Requests.Users.BalanceRequest.Queries;

public class GetBalanceByUserId : IRequest<Balance>
{
    public string UserId { get; set; }

    public GetBalanceByUserId(string userId)
    {
        if(userId == null)
            throw new ArgumentNullException(nameof(userId));
        UserId = userId;
    }
}


public class GetBalanceByUserIdHandler : IRequestHandler<GetBalanceByUserId, Balance>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<GetBalanceByUserIdHandler> _logger;

    public GetBalanceByUserIdHandler(ApplicationDbContext dbContext, ILogger<GetBalanceByUserIdHandler> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Balance?> Handle(GetBalanceByUserId request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Looking for user ID {id}", request.UserId);

        return await _dbContext.Balances
            .FirstOrDefaultAsync(b => b.UserId == request.UserId, cancellationToken);
    }
}