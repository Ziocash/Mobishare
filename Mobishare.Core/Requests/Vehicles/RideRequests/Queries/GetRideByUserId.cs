using System;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mobishare.Core.Data;
using Mobishare.Core.Models.Vehicles;

namespace Mobishare.Core.Requests.Vehicles.RideRequests.Queries;

public class GetRideByUserId : IRequest<Ride>
{
    public String UserId { get; }
    public GetRideByUserId(string userId)
    {
        UserId = userId ?? throw new ArgumentNullException(nameof(userId));
    }
}

public class GetRideByUserIdHandler : IRequestHandler<GetRideByUserId, Ride>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<GetRideByUserIdHandler> _logger;

    public GetRideByUserIdHandler(ApplicationDbContext dbContext, ILogger<GetRideByUserIdHandler> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Ride?> Handle(GetRideByUserId request, CancellationToken cancellationToken)
    {


        return await _dbContext.Rides
            .OrderByDescending(r => r.StartDateTime)
            .FirstOrDefaultAsync(r => r.UserId == request.UserId, cancellationToken);
    }
}