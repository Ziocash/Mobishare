using System;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Mobishare.Core.Models.Maps;
using Mobishare.Core.Requests.Maps.CityRequests.Commands;
using Mobishare.Core.Requests.Maps.CityRequests.Queries;
using Swashbuckle.AspNetCore.Annotations;

namespace Mobishare.Core.Controllers.Maps;

[ApiController]
[Route("api/[controller]")]
public class CityController : ControllerBase
{
    private readonly IMediator _mediator;

    public CityController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpPost("CreateCity")]
    [SwaggerOperation(
        Summary = "Create a new city",
        Description = "Creates and stores a new message in the specified conversation",
        OperationId = "CreateCity"
    )]
    [SwaggerResponse(201, "Message created successfully", typeof(CreateCity))]
    [SwaggerResponse(400, "Invalid request payload")]
    [SwaggerResponse(500, "Internal server error")]
    public async Task<IActionResult> CreateCity([FromBody] CreateCity request)
    {
        if (request == null)
        {
            return BadRequest("Request data cannot be null.");
        }

        var response = await _mediator.Send(request);
        if (response == null)
        {
            return StatusCode(500, "An error occurred while creating the city.");
        }

        return CreatedAtAction(nameof(CreateCity), new { id = response.Id }, response);
    }

    [HttpPut("UpdateCity")]
    [SwaggerOperation(
        Summary = "Update an existing city",
        Description = "Updates the details of an existing city",
        OperationId = "UpdateCity"
    )]
    [SwaggerResponse(200, "City updated successfully", typeof(UpdateCity))]
    [SwaggerResponse(400, "Invalid request payload")]
    [SwaggerResponse(404, "City not found")]
    [SwaggerResponse(500, "Internal server error")]
    public async Task<IActionResult> UpdateCity(
        [FromBody]
        [SwaggerRequestBody("City update request", Required = true)]
        UpdateCity request)
    {
        if (request == null)
        {
            return BadRequest("Request data cannot be null.");
        }

        var response = await _mediator.Send(request);
        if (response == null)
        {
            return NotFound("City not found.");
        }

        return Ok(response);
    }

    [HttpDelete("DeleteCity/{id}")]
    [SwaggerOperation(
        Summary = "Delete a city",
        Description = "Deletes a city by its ID",
        OperationId = "DeleteCity"
    )]
    [SwaggerResponse(204, "City deleted successfully")]
    [SwaggerResponse(404, "City not found")]
    [SwaggerResponse(500, "Internal server error")]
    public async Task<IActionResult> DeleteCity
    (
        [FromRoute]
        [SwaggerParameter("City ID", Required = true, Description = "The ID of the city to delete")]
        int id
    )
    {
        if (id <= 0)
        {
            return BadRequest("Invalid city ID.");
        }

        var response = await _mediator.Send(new DeleteCity { Id = id });
        if (response == null)
        {
            return NotFound("City not found.");
        }

        return NoContent();
    }

    [HttpGet("AllCities")]
    [SwaggerOperation(
        Summary = "Get all cities",
        Description = "Retrieves a list of all cities",
        OperationId = "GetAllCities"
    )]
    [SwaggerResponse(200, "List of cities retrieved successfully", typeof(IEnumerable<City>))]
    [SwaggerResponse(500, "Internal server error")]
    public async Task<IActionResult> GetAllCities()
    {
        var response = await _mediator.Send(new GetAllCities());
        if (response == null || !response.Any())
        {
            return NotFound("No cities found.");
        }

        return Ok(response);
    }
}
