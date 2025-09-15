using System;
using Microsoft.AspNetCore.Mvc;
using OllamaSharp;

namespace Mobishare.Ai.ChatBotAIService.ToolExecutor.Tools.RoutingTools;

public class RoutingTools
{
    private static readonly IRoutingTool _routingTool = new RoutingTool();

    [OllamaTool]
    public static async Task<IActionResult> RoutingPage(string route) 
        => await _routingTool.SimplePageRoutingAsync(route);
}
