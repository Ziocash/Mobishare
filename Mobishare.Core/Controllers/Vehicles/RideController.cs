using System;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Mobishare.Core.Models.Vehicles;
using Mobishare.Core.Requests.Vehicles.RideRequests.Commands;
using Mobishare.Core.Requests.Vehicles.RideRequests.Queries;
using Swashbuckle.AspNetCore.Annotations;

namespace Mobishare.Core.Controllers.Vehicles;

[ApiController]
[Route("api/[controller]")]
public class RideController : ControllerBase
{
    private readonly IMediator _mediator;

    public RideController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpPost("createRide")]
    [SwaggerOperation(
        Summary = "Create a new ride",
        Description = "This endpoint allows you to create a new ride for a vehicle."
    )]
    [SwaggerResponse(201, "Ride created successfully", typeof(CreateRide))]
    [SwaggerResponse(400, "Invalid request payload")]
    [SwaggerResponse(404, "Vehicle not found")]
    public async Task<IActionResult> CreateRide([FromBody] CreateRide createRide)
    {
        if (createRide == null)
        {
            return BadRequest("Invalid request payload.");
        }

        var result = await _mediator.Send(createRide);

        if (result == null)
        {
            return NotFound("Vehicle not found.");
        }

        return CreatedAtAction(nameof(CreateRide), new { id = result.Id }, result);
    }

    [HttpGet("AllRides")]
    [SwaggerOperation(
        Summary = "Get all rides",
        Description = "This endpoint retrieves all rides for a vehicle."
    )]
    [SwaggerResponse(200, "Rides retrieved successfully", typeof(IEnumerable<Ride>))]
    [SwaggerResponse(404, "No rides found")]
    public async Task<IActionResult> GetAllRides()
    {
        var result = await _mediator.Send(new GetAllRides());

        if (result == null || !result.Any())
        {
            return NotFound("No rides found.");
        }

        return Ok(result);
    }

    [HttpGet("AllUserRides/{userId}")]
    [SwaggerOperation(
        Summary = "Get all rides for a user",
        Description = "This endpoint retrieves all rides for a specific user.",
        OperationId = "GetAllUserRides"
    )]
    [SwaggerResponse(200, "User rides retrieved successfully", typeof(IEnumerable<Ride>))]
    [SwaggerResponse(404, "User not found or no rides found")]
    public async Task<IActionResult> GetAllUserRides(
        [FromRoute]
        [SwaggerParameter("userId", Required = true, Description = "The ID of the user whose rides are to be retrieved")]
        string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest("User ID cannot be null or empty.");
        }

        var result = await _mediator.Send(new GetAllUserRides(userId));
        if (result == null || !result.Any())
        {
            return NotFound("User not found or no rides found.");
        }
        return Ok(result);
    }

    [HttpGet("GetRide/{userId}")]
    [SwaggerOperation(
        Summary = "Get a specific ride for a user",
        Description = "This endpoint retrieves a specific ride for a user.",
        OperationId = "GetRideByUserId"
    )]
    [SwaggerResponse(200, "Ride retrieved successfully", typeof(Ride))]
    [SwaggerResponse(404, "Ride not found")]
    public async Task<IActionResult> GetRideByUserId(
        [FromRoute]
        [SwaggerParameter("userId", Required = true, Description = "The ID of the user whose ride is to be retrieved")]
        string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest("User ID cannot be null or empty.");
        }

        var result = await _mediator.Send(new GetRideByUserId(userId));

        if (result == null)
        {
            return NotFound("Ride not found.");
        }

        return Ok(result);
    }

    /*[HttpPut("updateRide")]
    [SwaggerOperation(
        Summary = "Update an existing ride",
        Description = "This endpoint allows you to update an existing ride for a vehicle."
    )]
    [SwaggerResponse(200, "Ride updated successfully", typeof(UpdateRide))]
    [SwaggerResponse(400, "Invalid request payload")]
    [SwaggerResponse(404, "Ride not found")]
    public async Task<IActionResult> UpdateRide(
        [FromBody] 
        [SwaggerParameter("updateRide", Required = true, Description = "The ride details to update")]
        UpdateRide updateRide)
    {
        if (updateRide == null)
        {
            return BadRequest("Invalid request payload.");
        }

        var result = await _mediator.Send(updateRide);

        if (result == null)
        {
            return NotFound("Ride not found.");
        }

        return Ok(result);
    }*/
}
