using System;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Mobishare.Core.Models.Vehicles;
using Mobishare.Core.Requests.Vehicles.PositionRequests.Commands;
using Mobishare.Core.Requests.Vehicles.PositionRequests.Queries;
using Swashbuckle.AspNetCore.Annotations;

namespace Mobishare.Core.Controllers.Vehicles;

[ApiController]
[Route("api/[controller]")]
public class PositionController : ControllerBase
{

    private readonly IMediator _mediator;

    public PositionController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpPost()]
    [SwaggerOperation(
        Summary = "Create a new position for a vehicle",
        Description = "Crreates a new position for a vehicle based on the provided details."
    )]
    [SwaggerResponse(201, "Position created successfully", typeof(Position))]
    [SwaggerResponse(400, "Invalid request data")]
    [SwaggerResponse(500, "Internal server error")]
    public async Task<IActionResult> CreatePosition([FromBody] CreatePosition request)
    {
        if (request == null)
        {
            return BadRequest("Request cannot be null.");
        }

        var result = await _mediator.Send(request);
        if (result == null)
        {
            return BadRequest("Failed to create position.");
        }

        return CreatedAtAction(nameof(CreatePosition), new { id = result.Id }, result);
    }

    [HttpGet("{vehicleId}")]
    [SwaggerOperation(
        Summary = "Get the current position of a vehicle",
        Description = "Retrieves the current position of a vehicle based on its ID."
    )]
    [SwaggerResponse(200, "Position retrieved successfully", typeof(Position))]
    [SwaggerResponse(404, "Position not found")]
    [SwaggerResponse(500, "Internal server error")]
    public async Task<IActionResult> GetPositionByVheicleId(
        [FromRoute]
        [SwaggerParameter("VehicleId", Required = true, Description = "The unique identifier of the vehicle.")]
        int VehicleId)
    {
        if (VehicleId <= 0)
        {
            return BadRequest("Invalid vehicle ID.");
        }

        var position = await _mediator.Send(new GetPositionByVehicleId(VehicleId));
        if (position == null)
        {
            return NotFound("Position not found for the given vehicle ID.");
        }

        return Ok(position);
    }
}
