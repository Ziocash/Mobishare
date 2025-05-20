using System;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Mobishare.Core.Data;
using Mobishare.Core.Models.Maps;
using Mobishare.Core.Requests.Maps.CityRequests.Commands;

namespace Mobishare.Core.Requests.Maps.ParkingSlotRequests.Commands;

public class CreateParkingSlot : ParkingSlot, IRequest<ParkingSlot>{}

public class CreateParkingSlotHandler : IRequestHandler<CreateParkingSlot, ParkingSlot>
{
    private readonly IMapper _mapper;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<CreateParkingSlotHandler> _logger;

    public CreateParkingSlotHandler(IMapper mapper, ApplicationDbContext dbContext, ILogger<CreateParkingSlotHandler> logger)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ParkingSlot> Handle(CreateParkingSlot request, CancellationToken cancellationToken)
    {
        var parkingSlot = _mapper.Map<ParkingSlot>(request);

        try
        {
            _logger.LogDebug("Executing {method}", nameof(CreateParkingSlot));
            _dbContext.ParkingSlots.Entry(parkingSlot).State = Microsoft.EntityFrameworkCore.EntityState.Added;
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Parking slot {parkingSlot} created successfully", parkingSlot);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating parking slot");
        }

        return parkingSlot;
    }
}