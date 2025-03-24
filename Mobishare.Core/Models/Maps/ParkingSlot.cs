using System;
using Microsoft.AspNetCore.Identity;
using NetTopologySuite.Geometries;

namespace Mobishare.Core.Models.Maps;

/// <summary>
/// This class is used to store the parking slot information.
/// </summary>
public class ParkingSlot
{
    public int Id {get; set;}
    public Polygon PerimeterLocation {get; set;}
    public DateTime CreatedAt {get; set;} = DateTime.UtcNow;
    public City City {get; set;}
    public int CityId {get; set;}
    public IdentityUser User {get; set;}
    public string UserId {get; set;}
}
