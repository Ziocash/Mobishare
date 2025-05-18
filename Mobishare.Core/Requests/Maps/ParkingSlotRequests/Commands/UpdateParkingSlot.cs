using System;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mobishare.Core.Data;
using Mobishare.Core.Models.Maps;

namespace Mobishare.Core.Requests.Maps.ParkingSlotRequests.Commands;

public class UpdateParkingSlot : ParkingSlot, IRequest<ParkingSlot> { }

public class UpdateParkingSlotHandler : IRequestHandler<UpdateParkingSlot, ParkingSlot>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<UpdateParkingSlotHandler> _logger;

    public UpdateParkingSlotHandler(ApplicationDbContext dbContext, ILogger<UpdateParkingSlotHandler> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ParkingSlot?> Handle(UpdateParkingSlot request, CancellationToken cancellationToken)
    {
        var parkingSlotToUpdate = await _dbContext.ParkingSlots.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (parkingSlotToUpdate == null)
        {
            _logger.LogWarning("Parking slot with ID {Id} not found", request.Id);
            return null;
        }

        try
        {
            _logger.LogDebug("Executing {method}", nameof(UpdateParkingSlotHandler));

            if (parkingSlotToUpdate.PerimeterLocation != request.PerimeterLocation)
                parkingSlotToUpdate.PerimeterLocation = request.PerimeterLocation;
            if (parkingSlotToUpdate.CityId != request.CityId)
                parkingSlotToUpdate.CityId = request.CityId;
            if (parkingSlotToUpdate.UserId != request.UserId)
                parkingSlotToUpdate.UserId = request.UserId;
            parkingSlotToUpdate.CreatedAt = request.CreatedAt;

            _dbContext.ParkingSlots.Update(parkingSlotToUpdate);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Parking slot {parkingSlotId} updated successfully", parkingSlotToUpdate.Id);
            return parkingSlotToUpdate;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating parking slot {parkingSlotId}", request.Id);
            throw;
        }
    }
}
