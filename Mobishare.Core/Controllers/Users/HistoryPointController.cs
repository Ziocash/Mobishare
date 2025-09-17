using System;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Mobishare.Core.Requests.Users.HistoryCreditRequest.Queries;
using Mobishare.Core.Requests.Users.HistoryPointRequest.Commands;
using Swashbuckle.AspNetCore.Annotations;

namespace Mobishare.Core.Controllers.Users;

[ApiController]
[Route("api/[controller]")]
public class HistoryPointController : ControllerBase
{
    private readonly IMediator _mediator;

    public HistoryPointController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpPost()]
    [SwaggerOperation(
        Summary = "Create a new history point",
        Description = "Creates and stores a new history point",
        OperationId = "CreateHistoryPoint"
    )]
    [SwaggerResponse(201, "Message created successfully", typeof(CreateHistoryPoint))]
    [SwaggerResponse(400, "Invalid request payload")]
    public async Task<IActionResult> CreateHistoryPoint([FromBody] CreateHistoryPoint request)
    {
        if (request == null)
        {
            return BadRequest("Request payload cannot be null.");
        }

        var result = await _mediator.Send(request);
        if (result == null)
        {
            return BadRequest("Failed to create history point.");
        }

        return CreatedAtAction(nameof(CreateHistoryPoint), new { id = result.Id }, result);
    }
}
