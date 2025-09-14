using System;
using Microsoft.AspNetCore.Identity;

namespace Mobishare.Core.Models.Chats;

public class ChatMessage
{
    public int Id { get; set; }
    public Conversation? Conversation { get; set; }
    public int ConversationId { get; set; }
    public string Sender { get; set; }
    public string Message { get; set; }
    public string Embedding { get; set; } // embedding of the message, used for RAG (Retrieval-Augmented Generation)
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
