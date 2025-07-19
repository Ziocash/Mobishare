using System;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Mobishare.Core.Requests.Chats.MessagePairRequests.Commands;
using Swashbuckle.AspNetCore.Annotations;

namespace Mobishare.Core.Controllers.Chats.ChatMessage;

[ApiController]
[Route("api/[controller]")]
public class MessagePairController : ControllerBase
{
    private readonly IMediator _mediator;

    public MessagePairController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpPost("CreateMessagePair")]
    [SwaggerOperation(
        Summary = "Create a Message Pair",
        Description = "Creates a new message pair in the chat system."
    )]
    [SwaggerResponse(201, "Message pair created successfully", typeof(CreateMessagePair))]
    [SwaggerResponse(400, "Invalid request data")]
    [SwaggerResponse(500, "Internal server error")]
    public async Task<IActionResult> CreateMessagePair([FromBody] CreateMessagePair request)
    {
        if (request == null) return BadRequest("Request data cannot be null.");

        var response = await _mediator.Send(request);

        if (response == null) return StatusCode(500, "An error occurred while creating the message pair.");

        return CreatedAtAction(nameof(CreateMessagePair), new { id = response.Id }, response);
    }
}