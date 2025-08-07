using System;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Mobishare.Core.Requests.Chats.ChatMessageRequests.Queries;
using Mobishare.Core.Requests.Chats.ConversationRequests.Commands;
using Mobishare.Core.Requests.Chats.ConversationRequests.Queries;
using Swashbuckle.AspNetCore.Annotations;

namespace Mobishare.Core.Controllers.Chats.ChatMessage;

[ApiController]
[Route("api/[controller]")]
public class ConversationController : ControllerBase
{
    private readonly IMediator _mediator;

    public ConversationController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpPost()]
    [SwaggerOperation(
        Summary = "Create a new conversation",
        Description = "Creates and stores a new conversation",
        OperationId = "CreateConversation"
    )]
    [SwaggerResponse(201, "Conversation created successfully", typeof(CreateConversation))]
    [SwaggerResponse(400, "Invalid request payload")]
    public async Task<IActionResult> CreateConversation(
        [FromBody]
        [SwaggerRequestBody("Conversation creation payload", Required = true)]
        CreateConversation command)
    {
        if (command == null) return BadRequest("Command cannot be null.");


        var result = await _mediator.Send(command);

        if (result == null) return NotFound("Conversation not found.");


        return CreatedAtAction(nameof(GetMessagesByConversationId), new { conversationId = result.Id }, result);
    }

    [HttpGet("Close/{conversationId}")]
    [SwaggerOperation(
        Summary = "Close a conversation",
        Description = "Closes a conversation by its ID.",
        OperationId = "CloseConversation"
    )]
    public async Task<IActionResult> CloseConversationById(
        [FromRoute]
        [SwaggerParameter("The ID of the conversation to close", Required = true)]
        int conversationId)
    {
        if (conversationId <= 0) return BadRequest("Invalid conversation ID.");

        var command = new CloseConversationById(conversationId);
        var result = await _mediator.Send(command);

        if (result == null) return NotFound("Conversation not found.");

        return NoContent();
    }


    [HttpGet("AllConversations")]
    [SwaggerOperation(
        Summary = "Get all conversations",
        Description = "Retrieves all conversations.",
        OperationId = "GetAllConversations"
    )]
    [SwaggerResponse(200, "List of conversations", typeof(IReadOnlyList<GetAllConversations>))]
    [SwaggerResponse(404, "No conversations found")]
    public async Task<IActionResult> GetAllConversations()
    {
        var query = new GetAllConversations();
        var conversations = await _mediator.Send(query);

        if (conversations == null || !conversations.Any())
        {
            return NotFound("No conversations found.");
        }

        return Ok(conversations);
    }

    [HttpGet("AllConversationsByUserId/{userId}")]
    [SwaggerOperation(
        Summary = "Get all conversations by user ID",
        Description = "Retrieves all conversations for a specific user.",
        OperationId = "GetAllConversationsByUserId"
    )]
    [SwaggerResponse(200, "List of conversations", typeof(IReadOnlyList<GetAllConversationsByUserId>))]
    [SwaggerResponse(404, "No conversations found for the user")]
    public async Task<IActionResult> GetAllConversationsByUserId(
        [FromRoute]
        [SwaggerParameter("User ID", Required = true)]
        string userId)
    {
        if (string.IsNullOrEmpty(userId)) return BadRequest("User ID cannot be null or empty.");

        var query = new GetAllConversationsByUserId(userId);
        var conversations = await _mediator.Send(query);

        if (conversations == null || !conversations.Any())
        {
            return NotFound("No conversations found for the user.");
        }

        return Ok(conversations);
    }

    [HttpGet("{conversationId}")]
    [SwaggerOperation(
        Summary = "Get Conversation by Conversation ID",
        Description = "Retrieves a conversation by its ID.",
        OperationId = "GetConversationById"
    )]
    [SwaggerResponse(200, "List of chat messages", typeof(IReadOnlyList<GetMessagesByConversationId>))]
    [SwaggerResponse(404, "No messages found for this conversation")]
    public async Task<IActionResult> GetConversationById(
        [FromRoute]
        [SwaggerParameter("Conversation ID", Required = true)]
        int conversationId)
    {
        var query = new GetConversationById(conversationId);
        var messages = await _mediator.Send(query);

        if (messages == null)
        {
            return NotFound("No messages found for this conversation.");
        }

        return Ok(messages);
    }
}