using System;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mobishare.Core.Data;
using Mobishare.Core.Models.Vehicles;

namespace Mobishare.Core.Requests.Vehicles.VehicleRequests.Queries;

public class GetVehicleById : IRequest<Vehicle>
{
    public int Id { get; set; }
}

public class GetVehicleByIdHandler : IRequestHandler<GetVehicleById, Vehicle>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<GetAllVehiclesHandler> _logger;

    public GetVehicleByIdHandler(ApplicationDbContext dbContext, ILogger<GetAllVehiclesHandler> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Vehicle?> Handle(GetVehicleById request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Executing {method}", nameof(GetAllVehicles));
            var vehicle = await _dbContext.Vehicles.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            return vehicle;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving vehicle");
            return null;
        }
    }
}