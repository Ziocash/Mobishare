using System;
using Microsoft.AspNetCore.Identity;

namespace Mobishare.Core.Models.Chats;

public class Conversation
{
    public int Id {get; set;}
    public DateTime CreatedAt {get; set;} = DateTime.UtcNow;
    public IdentityUser? User {get; set;}
    public string UserId {get; set;}
    public bool IsActive { get; set; }
}
