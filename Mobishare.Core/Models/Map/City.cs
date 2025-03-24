using System;
using Microsoft.AspNetCore.Identity;
using NetTopologySuite.Geometries;

namespace Mobishare.Core.Models.Map;

public class City
{
    public int Id {get; set;}
    public string Name{get; set;}
    public Polygon PermiterLocation {get; set;}
    public DateTime CreatedAt {get; set;} = DateTime.UtcNow;
    public IdentityUser User {get; set;}
    public string UserId {get; set;}
}
