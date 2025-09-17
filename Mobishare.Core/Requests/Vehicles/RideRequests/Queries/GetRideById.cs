using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mobishare.Core.Data;
using Mobishare.Core.Models.Vehicles;

namespace Mobishare.Core.Requests.Vehicles.RideRequests.Queries;

public class GetRideById : IRequest<Ride>
{
    public int RideId { get; set; }

    public GetRideById(int rideId)
    {
        RideId = rideId;
    }
}

public class GetRideByIdHandler : IRequestHandler<GetRideById, Ride>
{
    private readonly ApplicationDbContext _dbContext;
    
    public GetRideByIdHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<Ride> Handle(GetRideById request, CancellationToken cancellationToken)
    {
        return await _dbContext.Rides
            .FirstOrDefaultAsync(r => r.Id == request.RideId, cancellationToken);
    }
}