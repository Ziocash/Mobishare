using System;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Mobishare.Core.Requests.Chats.ChatMessageRequests.Commands;
using Mobishare.Core.Requests.Chats.ChatMessageRequests.Queries;
using Swashbuckle.AspNetCore.Annotations;

namespace Mobishare.Core.Controllers.Chats.ChatMessage;

[ApiController]
[Route("api/[controller]")]
public class ChatMessageController : ControllerBase
{
    private readonly IMediator _mediator;

    public ChatMessageController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpPost()]
    [SwaggerOperation(
        Summary = "Create a new chat message",
        Description = "Creates and stores a new message in the specified conversation",
        OperationId = "CreateChatMessage"
    )]
    [SwaggerResponse(201, "Message created successfully", typeof(CreateChatMessage))]
    [SwaggerResponse(400, "Invalid request payload")]
    public async Task<IActionResult> CreateChatMessage(
        [FromBody]
        [SwaggerRequestBody("Message creation payload", Required = true)]
        CreateChatMessage request)
    {
        if (request == null)
        {
            return BadRequest("Request cannot be null.");
        }

        var result = await _mediator.Send(request);
        return CreatedAtAction(nameof(GetMessagesByConversationId), new { conversationId = request.ConversationId }, result);
    }

    [HttpGet("{conversationId}")]
    [SwaggerOperation(
        Summary = "Get Messages by Conversation ID",
        Description = "Returns a list of chat messages for a specific conversation.",
        OperationId = "GetMessagesByConversationId"
    )]
    [SwaggerResponse(200, "List of chat messages", typeof(IReadOnlyList<GetMessagesByConversationId>))]
    [SwaggerResponse(404, "No messages found for this conversation")]
    public async Task<IActionResult> GetMessagesByConversationId(
        [FromRoute]
        [SwaggerParameter("Conversation ID", Required = true)]
        int conversationId)
    {
        var query = new GetMessagesByConversationId(conversationId);
        var messages = await _mediator.Send(query);

        if (messages == null || !messages.Any())
        {
            return NotFound();
        }

        return Ok(messages);
    }

}
