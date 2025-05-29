using System;
using Mobishare.Core.Models.Vehicles;

namespace Mobishare.Core.UiModels;

public class RideDisplayInfo
{
    public Ride Ride { get; set; }
    public string? StartLocationName { get; set; }
    public string? EndLocationName { get; set; }
}
