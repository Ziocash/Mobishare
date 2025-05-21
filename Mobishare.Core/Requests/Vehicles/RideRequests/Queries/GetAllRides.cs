using System;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mobishare.Core.Data;
using Mobishare.Core.Models.Vehicles;

namespace Mobishare.Core.Requests.Vehicles.RideRequests.Queries;

public class GetAllRides : IRequest<List<Ride>> { }

public class GetAllRidesHandler : IRequestHandler<GetAllRides, List<Ride>>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<GetAllRidesHandler> _logger;

    public GetAllRidesHandler(ApplicationDbContext dbContext, ILogger<GetAllRidesHandler> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<Ride>> Handle(GetAllRides request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Executing {method}", nameof(GetAllRides));
            var rides = await _dbContext.Rides
                .Include(r => r.PositionStart)
                .Include(r => r.PositionEnd)
                .Include(r => r.User)
                .Include(r => r.Vehicle)
                .ToListAsync(cancellationToken);
            return rides;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving rides");
            return new List<Ride>();
        }
    }
}