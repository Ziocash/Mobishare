using System;
using Mobishare.Core.Models.Vehicles;

namespace Mobishare.Core.Models.UserRelated;

public class TechnicianReports
{
    public string TechnicianId { get; set; }
    public int AssignedReports { get; set; }
    public IEnumerable<ReportSolution> LastClosedReports { get; set; }
}
