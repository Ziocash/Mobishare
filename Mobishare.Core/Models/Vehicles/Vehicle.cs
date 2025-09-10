using System;
using Mobishare.Core.Models.Maps;

namespace Mobishare.Core.Models.Vehicles;


/// <summary>
/// Vehicle entity.
/// </summary>
public class Vehicle
{
    public int Id { get; set; }
    public string Plate { get; set; }
    public string Status { get; set; }
    public double BatteryLevel { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ParkingSlot? ParkingSlot { get; set; }
    public int ParkingSlotId { get; set; }
    public VehicleType? VehicleType { get; set; }
    public int VehicleTypeId { get; set; }
    public ICollection<Position> Positions { get; set; } = new List<Position>(); 
}
