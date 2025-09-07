using System;
using Microsoft.AspNetCore.Identity;

namespace Mobishare.Core.Models.Vehicles;

/// <summary>
/// This class represents a repair record in the database.
/// </summary>
public class Repair
{
    public int Id {get; set;}
    public string Description {get; set;}
    public decimal Status {get; set;}
    public DateTime CreatedAt {get; set;} = DateTime.UtcNow;
    public DateTime? FinishedAt {get; set;}
    public IdentityUser? User {get; set;}
    public String UserId {get; set;}
    public Report? Report {get; set;}
    public int ReportId {get; set;}
}
