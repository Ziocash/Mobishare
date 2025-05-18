using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mobishare.Core.Data;
using Mobishare.Core.Models.Vehicles;

namespace Mobishare.Core.Requests.Vehicles.VehicleTypeRequests.Queries;

public class GetAllVehicleType : IRequest<List<VehicleType>> { }

public class GetAllVehicleTypeHandler : IRequestHandler<GetAllVehicleType, List<VehicleType>>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<GetAllVehicleTypeHandler> _logger;

    public GetAllVehicleTypeHandler(ApplicationDbContext dbContext, ILogger<GetAllVehicleTypeHandler> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<VehicleType>> Handle(GetAllVehicleType request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Executing {method}", nameof(GetAllVehicleType));
            var vehicleTypes = await _dbContext.VehicleTypes.ToListAsync(cancellationToken);
            return vehicleTypes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving vehicle types");
            return new List<VehicleType>();
        }
    }
}