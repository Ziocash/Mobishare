using System;

namespace Mobishare.Core.Models.Chats;

public class MessagePair
{
    public int Id { get; set; }
    public ChatMessage? UserMessage { get; set; } 
    public int? UserMessageId { get; set; } 
    public ChatMessage? AiMessage { get; set; }
    public int AiMessageId { get; set; }
    public bool IsForRag { get; set; } // if true, the message is used for RAG (Retrieval-Augmented Generation)
    public string SourceType { get; set; } // manual: aggiunto dall'utente, rag: generato da RAG
    public bool Answered { get; set; } // if true, the message has been answered by the bot, else ai cannnot answer it
    public string Language { get; set; } // ai language model used to answer the message
}
