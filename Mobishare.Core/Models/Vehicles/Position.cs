using System;

namespace Mobishare.Core.Models.Vehicles;

/// <summary>
/// This class represents a position record in the database.
/// /// </summary>
public class Position
{
    public int Id { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public DateTime GpsReceptionTime { get; set; } = DateTime.UtcNow;
    public DateTime GpsEmissionTime { get; set; } = DateTime.UtcNow;
    public Vehicle? Vehicle { get; set; }
    public int VehicleId { get; set; }
}
