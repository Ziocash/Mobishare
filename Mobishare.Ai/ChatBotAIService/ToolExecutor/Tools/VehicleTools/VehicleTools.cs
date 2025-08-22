using System;
using OllamaSharp;

namespace Mobishare.Ai.ChatBotAIService.ToolExecutor.Tools.VehicleTools;

public class VehicleTools
{
    public string? UserId { get; set; }

    private static readonly IVehicleTool _vehicleTool = new VehicleTool();

    [OllamaTool]
    public static async Task<string> ReportIssue(string description)
        => await _vehicleTool.ReportVehicleIssueAsync(description);

    [OllamaTool]
    public static async Task<string> ReserveVehicleAsync()
        => await _vehicleTool.ReserveVehicleAsync();
}
