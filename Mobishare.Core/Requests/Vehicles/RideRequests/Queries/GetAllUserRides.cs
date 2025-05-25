using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mobishare.Core.Data;
using Mobishare.Core.Models.Vehicles;

namespace Mobishare.Core.Requests.Vehicles.RideRequests.Queries;

public class GetAllUserRides : IRequest<List<Ride>>
{
    public string UserId { get; }
    public GetAllUserRides(string userId)
    {
        UserId = userId ?? throw new ArgumentNullException(nameof(userId));
    }
}

public class GetAllUserRidesHandler : IRequestHandler<GetAllUserRides, List<Ride>>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<GetAllUserRidesHandler> _logger;

    public GetAllUserRidesHandler(ApplicationDbContext dbContext, ILogger<GetAllUserRidesHandler> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<Ride>> Handle(GetAllUserRides request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Executing {method}", nameof(GetAllRides));
            if (request.UserId == null)
                return new List<Ride>();

            var rides = await _dbContext.Rides
                .Where(r => r.UserId == request.UserId)
                .Include(r => r.PositionStart)
                .Include(r => r.PositionEnd)
                .Include(r => r.User)
                .Include(r => r.Vehicle)
                .ToListAsync();
            return rides;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving rides");
            return new List<Ride>();
        }
    }
}