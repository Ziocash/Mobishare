using System;
using Microsoft.AspNetCore.Identity;

namespace Mobishare.Core.Models.Vehicles;

/// <summary>
/// This class represents a repair record in the database.
/// </summary>
public class RepairAssignment
{
    public IdentityUser User { get; set; }
    public string UserId { get; set; }
    public Repair Repair { get; set; }
    public int RepairId { get; set; }
}
