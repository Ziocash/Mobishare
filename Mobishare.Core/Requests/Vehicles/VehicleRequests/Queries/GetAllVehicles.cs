using System;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mobishare.Core.Data;
using Mobishare.Core.Models.Vehicles;

namespace Mobishare.Core.Requests.Vehicles.VehicleRequests.Queries;

public class GetAllVehicles : IRequest<List<Vehicle>> { }

public class GetAllVehiclesHandler : IRequestHandler<GetAllVehicles, List<Vehicle>>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<GetAllVehiclesHandler> _logger;

    public GetAllVehiclesHandler(ApplicationDbContext dbContext, ILogger<GetAllVehiclesHandler> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<Vehicle>> Handle(GetAllVehicles request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Executing {method}", nameof(GetAllVehicles));
            var vehicles = await _dbContext.Vehicles.ToListAsync(cancellationToken);
            return vehicles;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving vehicles");
            return new List<Vehicle>();
        }
    }
}