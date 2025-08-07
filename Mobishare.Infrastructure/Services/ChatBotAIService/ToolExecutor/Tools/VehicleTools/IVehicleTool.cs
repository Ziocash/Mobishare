using System;

namespace Mobishare.Infrastructure.Services.ChatBotAIService.ToolExecutor.Tools.VehicleTools;

public interface IVehicleTool
{
    /// <summary>
    /// Reports an issue with a vehicle.
    /// </summary>
    /// <param name="description"></param>
    /// 
    Task<string> ReportVehicleIssueAsync(string description);

    /// <summary>
    /// Reserves a vehicle.
    /// </summary>
    /// <returns></returns>
    Task<string> ReserveVehicleAsync();
}
