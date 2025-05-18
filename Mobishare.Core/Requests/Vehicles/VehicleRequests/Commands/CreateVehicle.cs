using System;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Mobishare.Core.Data;
using Mobishare.Core.Models.Vehicles;

namespace Mobishare.Core.Requests.Vehicles.VehicleRequests.Commands;

public class CreateVehicle : Vehicle, IRequest<Vehicle> { }

public class CreateVehicleHandler : IRequestHandler<CreateVehicle, Vehicle>
{
    private readonly IMapper _mapper;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<CreateVehicleHandler> _logger;

    public CreateVehicleHandler(IMapper mapper, ApplicationDbContext dbContext, ILogger<CreateVehicleHandler> logger)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handles the creation of a new vehicle.
    /// This method is responsible for adding a new vehicle to the database.
    /// It uses the provided request object to extract the necessary information for creating the vehicle.
    /// The method is asynchronous and returns a Task of type Vehicle.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Vehicle> Handle(CreateVehicle request, CancellationToken cancellationToken)
    {
        var newVehicle = _mapper.Map<Vehicle>(request);

        try
        {
            _logger.LogDebug("Executing {method}", nameof(CreateVehicle));
            _dbContext.Vehicles.Entry(newVehicle).State = Microsoft.EntityFrameworkCore.EntityState.Added;
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Vehicle {vehicle} created successfully", newVehicle);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating vehicle");
        }

        return newVehicle;
    }
}
