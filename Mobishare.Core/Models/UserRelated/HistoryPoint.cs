using System;
using Microsoft.AspNetCore.Identity;

namespace Mobishare.Core.Models.UserRelated;

public class HistoryPoint
{
    public int Id { get; set; }
    public int Point { get; set; }
    public IdentityUser? User {get; set;}
    public string UserId { get; set; }
    public Balance? Balance { get; set; }
    public int BalanceId { get; set; }
    public string TransactionType { get; set; } 
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
