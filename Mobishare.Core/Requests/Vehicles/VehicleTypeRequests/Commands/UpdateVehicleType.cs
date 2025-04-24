using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mobishare.Core.Data;
using Mobishare.Core.Models.Vehicles;

namespace Mobishare.Core.Requests.Vehicles.VehicleTypeRequests.Commands;

public class UpdateVehicleType : VehicleType, IRequest<VehicleType> { }

public class UpdateVehicleTypeHandler : IRequestHandler<UpdateVehicleType, VehicleType>
{
    private readonly IMapper _mapper;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<UpdateVehicleTypeHandler> _logger;

    public UpdateVehicleTypeHandler(IMapper mapper, ApplicationDbContext dbContext, ILogger<UpdateVehicleTypeHandler> logger)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<VehicleType?> Handle(UpdateVehicleType request, CancellationToken cancellationToken)
    {
        var vehicleType = _mapper.Map<VehicleType>(request);
        var vehicleTypeToUpdate = _dbContext.VehicleTypes.Where(x => x.Id == vehicleType.Id).FirstOrDefault();

        if (vehicleTypeToUpdate == null)
            _logger.LogWarning("Vehicle type with ID {Id} not found", request.Id);
        else
        {
            try
            {
                _logger.LogDebug("Executing {method}", nameof(UpdateVehicleType));
                
                if(vehicleTypeToUpdate.Model != vehicleType.Model)
                    vehicleTypeToUpdate.Model = vehicleType.Model;
                if(vehicleTypeToUpdate.Type != vehicleType.Type)
                    vehicleTypeToUpdate.Type = vehicleType.Type;
                if(vehicleTypeToUpdate.PricePerMinute != vehicleType.PricePerMinute)
                    vehicleTypeToUpdate.PricePerMinute = vehicleType.PricePerMinute;
                    
                vehicleTypeToUpdate.CreatedAt = vehicleType.CreatedAt;

                _dbContext.VehicleTypes.Update(vehicleTypeToUpdate).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Vehicle type {vehicleType} updated successfully", vehicleTypeToUpdate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating vehicle type {vehicleType}", vehicleTypeToUpdate);
            }
        }

        return vehicleTypeToUpdate;
    }
}