using System;
using Microsoft.AspNetCore.Mvc;

namespace Mobishare.Infrastructure.Services.ChatBotAIService.ToolExecutor.Tools.RoutingTools;

public interface IRoutingTool
{
    Task<IActionResult> SimplePageRoutingAsync(string route);
}
