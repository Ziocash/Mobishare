// GetVehicleTypeById.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mobishare.Core.Data;
using Mobishare.Core.Models.Vehicles;

namespace Mobishare.Core.Requests.Vehicles.VehicleTypeRequests.Queries;

public class GetVehicleTypeById : IRequest<VehicleType>
{
    public int VehicleTypeId { get; set; }

    public GetVehicleTypeById(int vehicleTypeId)
    {
        VehicleTypeId = vehicleTypeId;
    }
}

public class GetVehicleTypeByIdHandler : IRequestHandler<GetVehicleTypeById, VehicleType>
{
    private readonly ApplicationDbContext _dbContext;

    public GetVehicleTypeByIdHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<VehicleType> Handle(GetVehicleTypeById request, CancellationToken cancellationToken)
    {
        return await _dbContext.VehicleTypes
            .FirstOrDefaultAsync(vt => vt.Id == request.VehicleTypeId, cancellationToken);
    }
}