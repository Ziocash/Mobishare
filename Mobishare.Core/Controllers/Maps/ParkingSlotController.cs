using System;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Mobishare.Core.Models.Maps;
using Mobishare.Core.ParkingSlotClassification;
using Mobishare.Core.Requests.Maps.ParkingSlotRequests.Commands;
using Mobishare.Core.Requests.Maps.ParkingSlotRequests.Queries;
using Swashbuckle.AspNetCore.Annotations;


namespace Mobishare.Core.Controllers.Maps;

[ApiController]
[Route("api/[controller]")]
public class ParkingSlotController : ControllerBase
{
    private readonly IMediator _mediator;

    public ParkingSlotController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpPost]
    [SwaggerOperation(
        Summary = "Create a Parking Slot",
        Description = "Creates a new parking slot in the system."
    )]
    [SwaggerResponse(201, "Parking slot created successfully", typeof(CreateParkingSlot))]
    [SwaggerResponse(400, "Invalid request data")]
    [SwaggerResponse(500, "Internal server error")]
    public async Task<IActionResult> CreateParkingSlot(
        [FromBody]
        [SwaggerParameter("The details of the parking slot to create", Required = true)]
        CreateParkingSlot request)
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

        return CreatedAtAction(nameof(CreateParkingSlot), new { id = response.Id }, response);
    }

    [HttpPut]
    [SwaggerOperation(
        Summary = "Update a Parking Slot",
        Description = "Updates an existing parking slot in the system.",
        OperationId = "UpdateParkingSlot"

    )]
    [SwaggerResponse(200, "Parking slot updated successfully", typeof(UpdateParkingSlot))]
    [SwaggerResponse(400, "Invalid request data")]
    [SwaggerResponse(404, "Parking slot not found")]
    [SwaggerResponse(500, "Internal server error")]
    public async Task<IActionResult> UpdateParkingSlot
    (
        [FromBody]
        [SwaggerParameter("The ID of the parking slot to update", Required = true)]
        UpdateParkingSlot request
    )
    {
        if (request == null)
        {
            return BadRequest("Request data cannot be null.");
        }

        var response = await _mediator.Send(request);

        return Ok(response);
    }

    [HttpDelete("{id}")]
    [SwaggerOperation(
        Summary = "Delete a Parking Slot",
        Description = "Deletes a parking slot from the system.",
        OperationId = "DeleteParkingSlot"
    )]
    [SwaggerResponse(204, "Parking slot deleted successfully")]
    [SwaggerResponse(404, "Parking slot not found")]
    [SwaggerResponse(500, "Internal server error")]
    public async Task<IActionResult> DeleteParkingSlot
    (
        [FromRoute]
        [SwaggerParameter("The ID of the parking slot to delete", Required = true)]
        int id
    )
    {
        if (id <= 0)
        {
            return BadRequest("Invalid parking slot ID.");
        }
        var response = await _mediator.Send(new DeleteParkingSlot { Id = id });

        return NoContent();
    }

    [HttpGet("AllParkingSlots")]
    [SwaggerOperation(
        Summary = "Get all Parking Slots",
        Description = "Retrieves all parking slots in the system.",
        OperationId = "GetAllParkingSlots"
    )]
    [SwaggerResponse(200, "List of parking slots retrieved successfully", typeof(IEnumerable<ParkingSlot>))]
    [SwaggerResponse(500, "Internal server error")]
    public async Task<IActionResult> GetAllParkingSlots()
    {
        var response = await _mediator.Send(new GetAllParkingSlots());

        return Ok(response);
    }

    [HttpGet("AllAvailableParkingSlots")]
    [SwaggerOperation(
        Summary = "Get all Available Parking Slots",
        Description = "Retrieves all Available parking slots in the system.",
        OperationId = "GetAllAvailableParkingSlots"
    )]
    [SwaggerResponse(200, "List of parking slots retrieved successfully", typeof(IEnumerable<ParkingSlot>))]
    [SwaggerResponse(500, "Internal server error")]
    public async Task<IActionResult> GetAllAvailableParkingSlots()
    {
        var response = await _mediator.Send(new GetAllParkingSlots());
        response.Where(p => p.Type == ParkingSlotTypes.Available.ToString());

        return Ok(response);
    }
}
