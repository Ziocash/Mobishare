using System;

using MediatR;
using Microsoft.AspNetCore.Mvc;
using Mobishare.Core.Requests.Users.HistoryCreditRequest.Commands;
using Mobishare.Core.Requests.Users.HistoryCreditRequest.Queries;
using Swashbuckle.AspNetCore.Annotations;

namespace Mobishare.Core.Controllers.Users;

[ApiController]
[Route("api/[controller]")]
public class HistoryCreditController : ControllerBase
{
    private readonly IMediator _mediator;

    public HistoryCreditController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpPost()]
    [SwaggerOperation(
        Summary = "Create History Credit",
        Description = "Creates a new history credit record.")]
    public async Task<IActionResult> CreateHistoryCredit([FromBody] CreateHistoryCredit request)
    {
        if (request == null) return BadRequest("Request data cannot be null.");

        var response = await _mediator.Send(request);

        if (response == null) return StatusCode(500, "An error occurred while creating the history credit.");

        return CreatedAtAction(nameof(CreateHistoryCredit), new { id = response.Id }, response);
    }

    [HttpGet("AllHistoryCredits/{userId}")]
    [SwaggerOperation(
        Summary = "Get All History Credits",
        Description = "Retrieves all history credits for a specific user.")]
    public async Task<IActionResult> GetAllHistoryCreditsByUserId(
        [FromRoute]
        [SwaggerParameter("User ID", Required = true)]
        string userId)
    {
        if (string.IsNullOrEmpty(userId)) return BadRequest("User ID cannot be null or empty.");

        var query = new GetAllHistoryCreditByUserId(userId);
        var response = await _mediator.Send(query);

        if (response == null || !response.Any())
        {
            return NotFound("No history credits found for the specified user.");
        }

        return Ok(response);
    }
}
