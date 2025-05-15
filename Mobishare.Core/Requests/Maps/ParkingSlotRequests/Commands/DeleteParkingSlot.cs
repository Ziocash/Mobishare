using System;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mobishare.Core.Data;
using Mobishare.Core.Models.Maps;

namespace Mobishare.Core.Requests.Maps.ParkingSlotRequests.Commands;

public class DeleteParkingSlot : ParkingSlot, IRequest<ParkingSlot> { }

public class DeleteParkingSlotHandler : IRequestHandler<DeleteParkingSlot, ParkingSlot>
{
    private readonly IMapper _mapper;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<DeleteParkingSlotHandler> _logger;

    public DeleteParkingSlotHandler(IMapper mapper, ApplicationDbContext dbContext, ILogger<DeleteParkingSlotHandler> logger)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ParkingSlot> Handle(DeleteParkingSlot request, CancellationToken cancellationToken)
    {
        var parkingSlotId = _mapper.Map<ParkingSlot>(request);
        var parkingSlotToDelete = _dbContext.ParkingSlots.Where(x => x.Id == parkingSlotId.Id).First();

        if (parkingSlotToDelete == null)
        {
            _logger.LogWarning("Parking slot with ID {ParkingSlotId} not found", parkingSlotId.Id);
        }
        else
        {
            try
            {
                _logger.LogDebug("Executing {method}", nameof(DeleteParkingSlot));
                _dbContext.ParkingSlots.Remove(parkingSlotToDelete).State = EntityState.Deleted;
                await _dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Parking slot with ID {ParkingSlotId} deleted successfully", parkingSlotId.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting parking slot with ID {ParkingSlotId}", parkingSlotId.Id);
            }
        }

        return parkingSlotId;
    }
}
