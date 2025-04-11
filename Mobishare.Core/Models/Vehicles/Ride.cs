using System;
using Microsoft.AspNetCore.Identity;

namespace Mobishare.Core.Models.Vehicles;

/// <summary>
/// This class represents a ride record in the database.
/// </summary>
public class Ride
{
    public int Id {get; set;}
    public DateTime StartDateTime {get; set;}
    public DateTime EndDateTime {get; set;}
    public double Price {get; set;}
    public IdentityUser User {get; set;}
    public int UserId {get; set;}
    public Vehicle Vehicle {get; set;}
    public int VehicleId {get; set;}
}
