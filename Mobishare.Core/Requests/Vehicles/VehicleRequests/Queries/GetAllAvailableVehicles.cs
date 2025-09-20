using System;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mobishare.Core.Data;
using Mobishare.Core.Models.Vehicles;
using Mobishare.Core.VehicleStatus;
namespace Mobishare.Core.Requests.Vehicles.VehicleRequests.Queries;

public class GetAllAvailableVehicles : IRequest<List<Vehicle>> { }

public class GetAllAvailableVehiclesHandler : IRequestHandler<GetAllAvailableVehicles, List<Vehicle>>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<GetAllAvailableVehiclesHandler> _logger;

    public GetAllAvailableVehiclesHandler(ApplicationDbContext dbContext, ILogger<GetAllAvailableVehiclesHandler> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<Vehicle>> Handle(GetAllAvailableVehicles request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Executing {method}", nameof(GetAllAvailableVehicles));
            var vehicles = await _dbContext.Vehicles
                .Where(v => v.Status == VehicleStatusType.Free.ToString())
                .ToListAsync(cancellationToken);
            return vehicles;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving vehicles");
            return new List<Vehicle>();
        }
    }
}
