using System;
using Microsoft.AspNetCore.Identity;

namespace Mobishare.Core.Models.Vehicles;

/// <summary>
/// This class represents a repair record in the database.
/// </summary>
public class ReportAssignment
{
	public int Id { get; set; }
	public IdentityUser? User { get; set; }
    public string UserId { get; set; }
    public Report? Report { get; set; }
    public int ReportId { get; set; }
}
