using System;
using Microsoft.AspNetCore.Identity;
namespace Mobishare.Core.Models.User;

//<summary>
// This class is used to store the user's balances.
//</summary>
public class Balances
{
    public int Id {get; set;}
    public double Credit {get; set;}
    public int Points {get; set;}
    public IdentityUser User {get; set;}
    public string UserId {get; set;}
}
