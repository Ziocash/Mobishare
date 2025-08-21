using System;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mobishare.Core.Data;
using Mobishare.Core.Models.Vehicles;

namespace Mobishare.Core.Requests.Vehicles.VehicleRequests.Commands;

public class DeleteVehicle : Vehicle, IRequest<Vehicle> { }

public class DeleteVehicleHandler : IRequestHandler<DeleteVehicle, Vehicle>
{
    private readonly IMapper _mapper;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<DeleteVehicleHandler> _logger;

    public DeleteVehicleHandler(
        IMapper mapper,
        ApplicationDbContext dbContext,
        ILogger<DeleteVehicleHandler> logger)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Vehicle?> Handle(DeleteVehicle request, CancellationToken cancellationToken)
    {
        var vehicleId = _mapper.Map<Vehicle>(request);
        var vehicleToDelete = _dbContext.Vehicles.Where(x => x.Id == vehicleId.Id).First();

        if (vehicleToDelete == null)
            _logger.LogWarning("Vehicle with ID {Id} not found", request.Id);
        else
        {
            try
            {
                _logger.LogDebug("Executing {method}", nameof(DeleteVehicle));
                _dbContext.Vehicles.Remove(vehicleToDelete).State = EntityState.Deleted;
                await _dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Vehicle {vehicle} deleted successfully", vehicleToDelete);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting vehicle {vehicleType}", vehicleToDelete);
            }
        }

        return vehicleToDelete;
    }
}
