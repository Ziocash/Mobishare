using System;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mobishare.Core.Data;
using Mobishare.Core.Models.Maps;

namespace Mobishare.Core.Requests.Maps.ParkingSlotRequests.Queries;

public class GetAllParkingSlots : IRequest<List<ParkingSlot>> { }

public class GetAllParkingSlotsHandler : IRequestHandler<GetAllParkingSlots, List<ParkingSlot>>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<GetAllParkingSlotsHandler> _logger;

    public GetAllParkingSlotsHandler(ApplicationDbContext dbContext, ILogger<GetAllParkingSlotsHandler> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<ParkingSlot>> Handle(GetAllParkingSlots request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Executing {method}", nameof(GetAllParkingSlots));
            var parkingSlots = await _dbContext.ParkingSlots
                .Include(p => p.City) 
                .ToListAsync(cancellationToken);
            return parkingSlots;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving parking slots");
            return new List<ParkingSlot>();
        }
    }
}