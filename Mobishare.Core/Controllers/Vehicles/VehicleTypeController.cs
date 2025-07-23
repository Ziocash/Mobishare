using System;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Mobishare.Core.Models.Vehicles;
using Mobishare.Core.Requests.Vehicles.VehicleTypeRequests.Commands;
using Mobishare.Core.Requests.Vehicles.VehicleTypeRequests.Queries;
using Swashbuckle.AspNetCore.Annotations;
namespace Mobishare.Core.Controllers.Vehicles;

[ApiController]
[Route("api/[controller]")]
public class VehicleTypeController : ControllerBase
{
    private readonly IMediator _mediator;

    public VehicleTypeController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpPost()]
    [SwaggerOperation(
        Summary = "Create a new vehicle type",
        Description = "This endpoint allows you to create a new vehicle type."
    )]
    [SwaggerResponse(200, "Vehicle type created successfully", typeof(VehicleType))]
    [SwaggerResponse(400, "Invalid request data")]
    [SwaggerResponse(500, "Internal server error")]
    public async Task<IActionResult> CreateVehicleType([FromBody] CreateVehicleType request)
    {
        if (request == null)
        {
            return BadRequest("Request data cannot be null.");
        }

        var response = await _mediator.Send(request);
        if (response == null)
        {
            return StatusCode(500, "An error occurred while creating the parking slot.");
        }

        return CreatedAtAction(nameof(CreateVehicleType), new { id = response.Id }, response);
    }

    [HttpPut()]
    [SwaggerOperation(
        Summary = "Update an existing vehicle type",
        Description = "This endpoint allows you to update an existing vehicle type."
    )]
    [SwaggerResponse(200, "Vehicle type updated successfully", typeof(VehicleType))]
    [SwaggerResponse(400, "Invalid request data")]
    [SwaggerResponse(404, "Vehicle type not found")]
    [SwaggerResponse(500, "Internal server error")]
    public async Task<IActionResult> UpdateVehicleType(
        [FromBody]
        [SwaggerParameter("The ID of the vehicle type to update", Required = true)]
        UpdateVehicleType request)
    {
        if (request == null || request.Id <= 0)
        {
            return BadRequest("Invalid request data.");
        }

        var response = await _mediator.Send(request);
        if (response == null)
        {
            return NotFound($"Vehicle type with ID {request.Id} not found.");
        }

        return Ok(response);
    }

    [HttpDelete("{id}")]
    [SwaggerOperation(
        Summary = "Delete a vehicle type",
        Description = "This endpoint allows you to delete a vehicle type by its ID."
    )]
    [SwaggerResponse(200, "Vehicle type deleted successfully")]
    [SwaggerResponse(404, "Vehicle type not found")]
    [SwaggerResponse(500, "Internal server error")]
    public async Task<IActionResult> DeleteVehicleType(
        [FromRoute]
        [SwaggerParameter("The ID of the vehicle type to delete", Required = true)]
        int id)
    {
        if (id <= 0)
        {
            return BadRequest("Invalid vehicle type ID.");
        }

        var response = await _mediator.Send(new DeleteVehicleType { Id = id });
        if (response == null)
        {
            return NotFound($"Vehicle type with ID {id} not found.");
        }

        return NoContent();
    }
    

    [HttpGet("AllVehicleTypes")]
    [SwaggerOperation(
        Summary = "Get all vehicle types",
        Description = "Retrieves a list of all vehicle types available in the system."
    )]
    [SwaggerResponse(200, "Vehicle types retrieved successfully", typeof(IEnumerable<VehicleType>))]
    [SwaggerResponse(404, "No vehicle types found")]
    [SwaggerResponse(500, "Internal server error")]
    public async Task<IActionResult> GetAllVehicleTypes()
    {
        var response = await _mediator.Send(new GetAllVehicleType());
        if (response == null || !response.Any())
        {
            return NotFound("No vehicle types found.");
        }

        return Ok(response);
    }

    
}
