using System;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mobishare.Core.Data;
using Mobishare.Core.Models.Vehicles;


namespace Mobishare.Core.Requests.Vehicles.PositionRequests.Queries;

public class GetPositionByVehicleId : IRequest<Position>
{
    public int VehicleId { get; set; }

    public GetPositionByVehicleId(int vehicleId)
    {
        if (vehicleId <= 0)
            throw new ArgumentOutOfRangeException(nameof(vehicleId));
        VehicleId = vehicleId;
    }
}

public class GetPositionByVehicleIdHandler : IRequestHandler<GetPositionByVehicleId, Position>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<GetPositionByVehicleIdHandler> _logger;

    public GetPositionByVehicleIdHandler(ApplicationDbContext dbContext, ILogger<GetPositionByVehicleIdHandler> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Position?> Handle(GetPositionByVehicleId request, CancellationToken cancellationToken)
    {
       
         _logger.LogDebug("Looking for vehicle ID {id}", request.VehicleId);
        

        return await _dbContext.Positions
            .OrderBy(p => p.GpsReceptionTime)
            .LastOrDefaultAsync(p => p.VehicleId == request.VehicleId, cancellationToken);
            
    
    }
}

