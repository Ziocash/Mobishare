using System;

namespace Mobishare.Core.Models.Vehicles;

/// <summary>
/// VehicleType entity.
/// </summary>
public class VehicleType
{
    public int Id { get; set; }
    public string Model { get; set; }
    public string Type { get; set; }
    public double PricePerMinute { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
