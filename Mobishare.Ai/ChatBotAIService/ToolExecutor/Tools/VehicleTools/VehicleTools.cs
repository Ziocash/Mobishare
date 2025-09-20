using System;
using OllamaSharp;

namespace Mobishare.Ai.ChatBotAIService.ToolExecutor.Tools.VehicleTools;

public class VehicleTools
{
    public string? UserId { get; set; }

    private static readonly IVehicleTool _vehicleTool = new VehicleTool();

    [OllamaTool]
    public static async Task<string> ReportIssue(string description, int vehicleId)
        => await _vehicleTool.ReportVehicleIssueAsync(description, vehicleId);

    [OllamaTool]
    public static async Task<string> ReserveVehicleAsync(string userRequest)
        => await _vehicleTool.ReserveVehicleAsync(userRequest);
}
