using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mobishare.Core.Models.Chats;
using Mobishare.Core.Requests.Chats.ChatMessageRequests.Queries;
using Mobishare.Core.Requests.Chats.ConversationRequests.Commands;
using Mobishare.Core.Requests.Chats.ConversationRequests.Queries;

namespace Mobishare.App.Pages
{
    public class ChatModel : PageModel
    {
        private readonly ILogger<ChatModel> _logger;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly UserManager<IdentityUser> _userManager;
        public Conversation CurrentConversation { get; set; } = null;
        public IEnumerable<ChatMessage> Messages { get; set; } = new List<ChatMessage>();

        public ChatModel(
            ILogger<ChatModel> logger,
            IMediator mediator,
            IMapper mapper,
            UserManager<IdentityUser> userManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public async Task<IActionResult> OnGet()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID is null or empty.");
                return RedirectToPage("/LandingPage");
            }

            var conversations = await _mediator.Send(new GetAllConversationsByUserId(userId));
            conversations = conversations.OrderBy(c => c.CreatedAt);
            
            if(conversations == null || !conversations.Any())
            {
                var response = await _mediator.Send(new CreateConversation{
                    UserId = userId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
                if (response != null)
                {
                    CurrentConversation = response;
                    _logger.LogInformation("New conversation created with ID: {ConversationId}", response.Id);
                } 
                else
                {
                    _logger.LogError("Failed to create a new conversation for user ID: {UserId}", userId);
                    return RedirectToPage("/LandingPage");
                }
            } else if (conversations.Count() > 0)
            {
                CurrentConversation = conversations.FirstOrDefault();
                _logger.LogInformation("Existing conversation found with ID: {ConversationId}", CurrentConversation.Id);
                Messages = await _mediator.Send(new GetMessagesByConversationId(CurrentConversation.Id));
                Messages = Messages.OrderBy(m => m.CreatedAt);
            }

            return Page();
        }
    }
}
