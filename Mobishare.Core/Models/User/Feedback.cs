using System;
using Microsoft.AspNetCore.Identity;

namespace Mobishare.Core.Models.User;

//<summary>
// This class is used to store the user's feedback about this website services.
//</summary>
public class Feedback
{
    public int Id {get; set;}
    public string Message {get; set;}
    public DateTime CreatedAt {get; set;} = DateTime.UtcNow;
    public float Rating {get; set;}
    public IdentityUser User {get; set;}
    public string UserId {get; set;}
}
