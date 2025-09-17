using System;
using System.Reflection;
using Microsoft.AspNetCore.Identity;
using static System.Net.Mime.MediaTypeNames;

namespace Mobishare.Core.Models.Vehicles;

/// <summary>
/// This class represents a report record in the database.
/// </summary>
public class Report
{
    public int Id { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? Image { get; set; }
    public IdentityUser? User { get; set; }
    public string UserId { get; set; }
    public Vehicle? Vehicle { get; set; }
    public int VehicleId { get; set; }
    public string? Status { get; set; }

    public ICollection<Repair> Repairs { get; set; } = new List<Repair>();
}
