using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Mobishare.Core.Data;
using Mobishare.Core.Models.Vehicles;

public class CreateVehicleType : VehicleType, IRequest<VehicleType> { }

public class CreateVehicleTypeHandler : IRequestHandler<CreateVehicleType, VehicleType>
{
    private readonly IMapper _mapper;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<CreateVehicleTypeHandler> _logger;

    public CreateVehicleTypeHandler(IMapper mapper, ApplicationDbContext dbContext, ILogger<CreateVehicleTypeHandler> logger)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handles the creation of a new vehicle type.
    /// This method is responsible for adding a new vehicle type to the database.
    /// It uses the provided request object to extract the necessary information for creating the vehicle type.
    /// The method is asynchronous and returns a Task of type VehicleType.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<VehicleType> Handle(CreateVehicleType request, CancellationToken cancellationToken)
    {
        var newVehicleType = _mapper.Map<VehicleType>(request);

        try
        {
            _logger.LogDebug("Executing {method}", nameof(CreateVehicleType));
            _dbContext.VehicleTypes.Entry(newVehicleType).State = Microsoft.EntityFrameworkCore.EntityState.Added;
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Vehicle type {vehicleType} created successfully", newVehicleType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating vehicle type {vehicleType}", newVehicleType);
        }

        return newVehicleType;
    }
}