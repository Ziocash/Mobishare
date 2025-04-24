using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mobishare.Core.Data;
using Mobishare.Core.Models.Vehicles;

namespace Mobishare.Core.Requests.Vehicles.VehicleTypeRequests.Commands;

public class DeleteVehicleType : VehicleType, IRequest<VehicleType> { }

public class DeleteVehicleTypeHandler : IRequestHandler<DeleteVehicleType, VehicleType>
{
    private readonly IMapper _mapper;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<DeleteVehicleTypeHandler> _logger;

    public DeleteVehicleTypeHandler(IMapper mapper, ApplicationDbContext dbContext, ILogger<DeleteVehicleTypeHandler> logger)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<VehicleType?> Handle(DeleteVehicleType request, CancellationToken cancellationToken)
    {
        var vehicleTypeId = _mapper.Map<VehicleType>(request);
        var vehicleTypeToDelete = _dbContext.VehicleTypes.Where(x => x.Id == vehicleTypeId.Id).First();

        if (vehicleTypeToDelete == null)
            _logger.LogWarning("Vehicle type with ID {Id} not found", request.Id);
        else
        {
            try
            {
                _logger.LogDebug("Executing {method}", nameof(DeleteVehicleType));
                _dbContext.VehicleTypes.Remove(vehicleTypeToDelete).State = EntityState.Deleted;
                await _dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Vehicle type {vehicleType} deleted successfully", vehicleTypeToDelete);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting vehicle type {vehicleType}", vehicleTypeToDelete);
            }
        }

        return vehicleTypeToDelete; ;
    }
}