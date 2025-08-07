using System;
using Microsoft.AspNetCore.Mvc;

namespace Mobishare.Infrastructure.Services.ChatBotAIService.ToolExecutor.Tools.RoutingTools;

public class RoutingTool : IRoutingTool
{
    public Task<IActionResult> SimplePageRoutingAsync(string route)
    {
       return Task.FromResult<IActionResult>(new RedirectToPageResult($"/{route}"));
    }
}
