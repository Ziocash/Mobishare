using System;
using Mobishare.Core.Models.Maps;

namespace Mobishare.Core.Models.Vehicles;


/// <summary>
/// Vehicle entity.
/// </summary>
public class Vehicle
{
    public int Id {get; set;}
    public string Plate {get; set;}
    public string Model {get; set;}
    public string Type {get; set;}
    public double PricePerMinute {get; set;}
    public string Status {get; set;}
    public double BatteryLevel {get; set;}
    public DateTime CreatedAt {get; set;} = DateTime.UtcNow;
    public ParkingSlot ParkingSlot {get; set;}
    public string ParkingSlotId {get; set;}
}
