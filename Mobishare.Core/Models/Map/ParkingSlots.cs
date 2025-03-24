using System;
using Microsoft.AspNetCore.Identity;
using NetTopologySuite.Geometries;

namespace Mobishare.Core.Models.Map;

public class ParkingSlots
{
    public int Id {get; set;}
    public Polygon PermiterLocation {get; set;}
    public DateTime CreatedAt {get; set;} = DateTime.UtcNow;
    public Cities City {get; set;}
    public int CityId {get; set;}
    public IdentityUser User {get; set;}
    public string UserId {get; set;}
}
