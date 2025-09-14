using System;

namespace Mobishare.Ai.ChatBotAIService.ToolExecutor.Tools.VehicleTools;

public interface IVehicleTool
{
    /// <summary>
    /// Reports an issue with a vehicle.
    /// </summary>
    /// <param name="description"></param>
    /// <param name="vehicleId"></param>
    /// 
    Task<string> ReportVehicleIssueAsync(string description, int vehicleId);

    /// <summary>
    /// Reserves a vehicle.
    /// </summary>
    /// <returns></returns>
    Task<string> ReserveVehicleAsync();
}
