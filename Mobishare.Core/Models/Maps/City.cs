using System;
using Microsoft.AspNetCore.Identity;

namespace Mobishare.Core.Models.Maps;

/// <summary>
/// This class is used to store the city information.
/// </summary>
public class City
{
    public int Id {get; set;}
    public string Name{get; set;}
    public string PerimeterLocation {get; set;}
    public DateTime CreatedAt {get; set;} = DateTime.UtcNow;
    public IdentityUser? User {get; set;}
    public string UserId {get; set;}
}
