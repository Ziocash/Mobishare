using System;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Mobishare.Core.Requests.Chats.ChatMessageRequests.Queries;



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

    /*[HttpPost("CreateChatMessage")]
    public async Task<IActionResult> CreateChatMessage([FromBody] CreateChatMessageRequest request)
    {
        if (request == null)
        {
            return BadRequest("Request cannot be null.");
        }

        var result = await _mediator.Send(request);
        return CreatedAtAction(nameof(GetMessagesByConversationId), new { conversationId = request.ConversationId }, result);
    }*/

    [HttpGet("{conversationId}")]
    public async Task<IActionResult> GetMessagesByConversationId(int conversationId)
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
