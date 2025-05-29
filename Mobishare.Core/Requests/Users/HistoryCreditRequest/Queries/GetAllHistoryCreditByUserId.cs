using System;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mobishare.Core.Data;
using Mobishare.Core.Models.UserRelated;

namespace Mobishare.Core.Requests.Users.HistoryCreditRequest.Queries;

public class GetAllHistoryCreditByUserId : IRequest<List<HistoryCredit>>
{
    public string UserId { get; set; }

    public GetAllHistoryCreditByUserId(string userId)
    {
        if (userId == null)
            throw new ArgumentNullException(nameof(userId));
        UserId = userId;
    }
}

public class GetAllHistoryCreditByUserIdHandler : IRequestHandler<GetAllHistoryCreditByUserId, List<HistoryCredit>>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<GetAllHistoryCreditByUserIdHandler> _logger;

    public GetAllHistoryCreditByUserIdHandler(ApplicationDbContext dbContext, ILogger<GetAllHistoryCreditByUserIdHandler> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<HistoryCredit>> Handle(GetAllHistoryCreditByUserId request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Looking for user ID {id}", request.UserId);

        return await _dbContext.HistoryCredits
            .Where(x => x.UserId == request.UserId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}