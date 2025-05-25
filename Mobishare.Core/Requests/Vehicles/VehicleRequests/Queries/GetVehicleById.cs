using System;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mobishare.Core.Data;
using Mobishare.Core.Models.Vehicles;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Mobishare.Core.Requests.Vehicles.VehicleRequests.Queries;


public class GetVehicleById : IRequest<Vehicle>
{
    public int VehicleId { get; set; }

    public GetVehicleById(int vehicleId)
    {
        if (vehicleId <= 0)
            throw new ArgumentOutOfRangeException(nameof(vehicleId));
        VehicleId = vehicleId;
    }
}

public class GetVehicleByIdHandler : IRequestHandler<GetVehicleById, Vehicle>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<GetAllVehiclesHandler> _logger;

    public GetVehicleByIdHandler(IServiceScopeFactory serviceScopeFactory, ILogger<GetAllVehiclesHandler> logger)
    {
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

     public async Task<Vehicle?> Handle(GetVehicleById request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Looking for vehicle ID {id}", request.VehicleId);
       
        using var scope = _serviceScopeFactory.CreateScope();
        var _dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var vehicle = await _dbContext.Vehicles
            .FirstOrDefaultAsync(v => v.Id == request.VehicleId, cancellationToken);
        return vehicle;
    }
}
//-------------------------------------------------------------------------------------
// Definition of the API controller
// [ApiController]
// [Route("api/vehicles")]
// public class VehiclesController : ControllerBase
// {
//     private readonly IMediator _mediator; // for comunication between the controller and the application layer

//     public VehiclesController(IMediator mediator)
//     {
//         _mediator = mediator;
//     }

//     [HttpGet("{id}")]
//     public async Task<IActionResult> GetVehicleById(int id)
//     {
//         var vehicle = await _mediator.Send(new GetVehicleById(id)); // Send the request to the mediator
//         if (vehicle == null)
//         {
//             return NotFound();
            
//         }
//         return Ok(vehicle);
//     }
// }