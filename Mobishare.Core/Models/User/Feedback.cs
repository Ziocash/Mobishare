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
    public DateTime CreatedAt {get; set;}
    public float Rating {get; set;}
    public int UserId {get; set;}
    public IdentityUser User {get; set;}
}
