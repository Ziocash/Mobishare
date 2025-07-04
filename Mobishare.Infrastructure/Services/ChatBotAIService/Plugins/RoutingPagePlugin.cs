using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace Mobishare.Infrastructure.Services.ChatBotAIService.Pulgins;

public class RoutingPagePlugin
{
    [KernelFunction, Description("Vai alla pagina del portafoglio")]
    public string RouteToWallet() => "/wallet";

    [KernelFunction, Description("Vai alla pagina dei veicoli")]
    public string RouteToBooking() => "/vehicles";  // Modificato per coerenza con UI

    [KernelFunction, Description("Vai alla pagina di supporto")]
    public string RouteToSupport() => "/support";

    [KernelFunction, Description("Vai alla home page")]
    public string RouteToHome() => "/home";
}