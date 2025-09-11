using System;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Mobishare.Core.Models.Vehicles;
using Mobishare.Core.Requests.Vehicles.RepairRequests.Commands;
using Swashbuckle.AspNetCore.Annotations;

namespace Mobishare.Core.Controllers.Vehicles;

[ApiController]
[Route("api/[controller]")]
public class RepairController : ControllerBase
{
    private readonly IMediator _mediator;

    public RepairController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpPost()]
    [SwaggerOperation(
        Summary = "Create a new repair",
        Description = "Crreates a new repair."
    )]
    [SwaggerResponse(201, "Repair created successfully", typeof(Repair))]
    [SwaggerResponse(400, "Invalid request data")]
    [SwaggerResponse(500, "Internal server error")]
    public async Task<IActionResult> CreateRepair([FromBody] CreateRepair request)
    {
        if (request == null)
        {
            return BadRequest("Request cannot be null.");
        }

        var result = await _mediator.Send(request);
        if (result == null)
        {
            return BadRequest("Failed to create repair.");
        }

        return CreatedAtAction(nameof(CreateRepair), new { id = result.Id }, result);
    }
}
