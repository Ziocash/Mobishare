using System;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Mobishare.Core.Data;
using Mobishare.Core.Models.Vehicles;

namespace Mobishare.Core.Requests.Vehicles.RideRequests.Commands;

public class CreateRide : Ride, IRequest<Ride> { }

public class CreateRideHandler : IRequestHandler<CreateRide, Ride>
{
    private readonly IMapper _mapper;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<CreateRideHandler> _logger;

    public CreateRideHandler(IMapper mapper, ApplicationDbContext dbContext, ILogger<CreateRideHandler> logger)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handles the creation of a new ride.
    /// This method maps the request to a Ride entity, sets its state to Added in the DbContext,
    /// saves the changes to the database, and returns the created Ride entity.
    /// It also logs the process, including any errors that may occur during the operation.
    /// The method is asynchronous and can be cancelled using a CancellationToken.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Ride> Handle(CreateRide request, CancellationToken cancellationToken)
    {
        var newRide = _mapper.Map<Ride>(request);
        
        try
        {
            _logger.LogDebug("Executing {method}", nameof(CreateRide));
            _dbContext.Rides.Entry(newRide).State = Microsoft.EntityFrameworkCore.EntityState.Added;
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Ride {RideId} created successfully", newRide);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating new ride");

        }
        return newRide;
    }
}