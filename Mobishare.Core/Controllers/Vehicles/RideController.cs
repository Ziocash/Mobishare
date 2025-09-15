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

    [HttpPost()]
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
        
        return Ok(result);
    }

    [HttpGet("User/{userId}")]
    [SwaggerOperation(
        Summary = "Get the last ride for a specific user",
        Description = "This endpoint retrieves the last ride for a specific user.",
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

    [HttpGet("{rideId}")]
    [SwaggerOperation(
        Summary = "Get a ride by its ID",
        Description = "This endpoint retrieves a ride by its unique identifier.",
        OperationId = "GetRideById"
    )]
    [SwaggerResponse(200, "Ride retrieved successfully", typeof(Ride))]
    [SwaggerResponse(404, "Ride not found")]
    public async Task<IActionResult> GetRideById(
        [FromRoute]
        [SwaggerParameter("rideId", Required = true, Description = "The ID of the ride to retrieve")]
        int rideId)
    {
        if (rideId <= 0)
        {
            return BadRequest("Ride ID cannot be null or empty.");
        }
        var result = await _mediator.Send(new GetRideById(rideId));
        if (result == null)
        {
            return NotFound("Ride not found.");
        }
        return Ok(result);
    }
    

    [HttpPut()]
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
    }
}
