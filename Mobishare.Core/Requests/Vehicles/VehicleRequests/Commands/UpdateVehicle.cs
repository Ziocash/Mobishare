using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mobishare.Core.Data;
using Mobishare.Core.Models.Maps;
using Mobishare.Core.Models.Vehicles;

namespace Mobishare.Core.Requests.Vehicles.VehicleRequests.Commands
{
    public class UpdateVehicle : Vehicle, IRequest<Vehicle> { }

    public class UpdateVehicleHandler : IRequestHandler<UpdateVehicle, Vehicle>
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<UpdateVehicleHandler> _logger;

        public UpdateVehicleHandler(ApplicationDbContext dbContext, ILogger<UpdateVehicleHandler> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Handles the update of a city in the database.
        /// /// </summary>
        /// <param name="request">The request containing the city data to update.</param>
        public async Task<Vehicle?> Handle(UpdateVehicle request, CancellationToken cancellationToken)
        {
            var vehicleToUpdate = await _dbContext.Vehicles.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (vehicleToUpdate == null)
            {
                _logger.LogWarning("City with ID {Id} not found", request.Id);
                return null;
            }

            try
            {
                _logger.LogDebug("Executing {method}", nameof(UpdateVehicleHandler));

                if (vehicleToUpdate.Plate != request.Plate)
                    vehicleToUpdate.Plate = request.Plate;
                if (vehicleToUpdate.Status != request.Status)
                    vehicleToUpdate.Status = request.Status;
                if (vehicleToUpdate.BatteryLevel != request.BatteryLevel)
                    vehicleToUpdate.BatteryLevel = request.BatteryLevel;
                if (vehicleToUpdate.ParkingSlotId != request.ParkingSlotId)
                    vehicleToUpdate.ParkingSlotId = request.ParkingSlotId;
                if (vehicleToUpdate.VehicleTypeId != request.VehicleTypeId)
                    vehicleToUpdate.VehicleTypeId = request.VehicleTypeId;
                
                vehicleToUpdate.CreatedAt = request.CreatedAt;

                _dbContext.Vehicles.Update(vehicleToUpdate);
                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Vehicle {cityId} updated successfully", vehicleToUpdate.Id);
                return vehicleToUpdate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating city {cityId}", request.Id);
                throw;
            }
        }
    }
}
