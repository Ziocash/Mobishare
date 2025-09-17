using System;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Mobishare.Core.Requests.Users.BalanceRequest.Commands;
using Mobishare.Core.Requests.Users.BalanceRequest.Queries;
using Swashbuckle.AspNetCore.Annotations;

namespace Mobishare.Core.Controllers.Users;

[ApiController]
[Route("api/[controller]")]
public class BalanceController : ControllerBase
{
    private readonly IMediator _mediator;

    public BalanceController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpPost()]
    [SwaggerOperation(
        Summary = "Create a new balance",
        Description = "Creates and stores a new balance",
        OperationId = "CreateBalance"
    )]
    [SwaggerResponse(201, "Message created successfully", typeof(CreateBalance))]
    [SwaggerResponse(400, "Invalid request payload")]
    public async Task<IActionResult> CreateBalance()
    {
        var request = new CreateBalance();
        if (request == null)
        {
            return BadRequest("Request payload cannot be null.");
        }

        var result = await _mediator.Send(request);
        if (result == null)
        {
            return BadRequest("Failed to create balance.");
        }

        return CreatedAtAction(nameof(GetBalanceByUserId), new { userId = result.UserId }, result);
    }

    [HttpPut()]
    [SwaggerOperation(
        Summary = "Update an existing balance",
        Description = "Updates an existing balance",
        OperationId = "UpdateBalance"
    )]
    [SwaggerResponse(200, "Balance updated successfully", typeof(UpdateBalance))]
    [SwaggerResponse(400, "Invalid request payload")]
    public async Task<IActionResult> UpdateBalance(
        [FromBody]
        [SwaggerRequestBody("Balance update payload", Required = true)]
        UpdateBalance request)
    {
        if (request == null)
        {
            return BadRequest("Request payload cannot be null.");
        }

        var result = await _mediator.Send(request);

        return Ok(result);
    }

    [HttpGet("{userId}")]
    [SwaggerOperation(
        Summary = "Get balance by user ID",
        Description = "Retrieves the balance for a specific user",
        OperationId = "GetBalanceByUserId"
    )]
    [SwaggerResponse(200, "Balance retrieved successfully", typeof(CreateBalance))]
    [SwaggerResponse(404, "Balance not found for the specified user ID")]
    public async Task<IActionResult> GetBalanceByUserId(
        [FromRoute]
        [SwaggerParameter("The ID of the user whose balance is to be retrieved", Required = true)]
        string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest("User ID cannot be null or empty.");
        }

        var result = await _mediator.Send(new GetBalanceByUserId(userId));
        
        return Ok(result);
    }
}
