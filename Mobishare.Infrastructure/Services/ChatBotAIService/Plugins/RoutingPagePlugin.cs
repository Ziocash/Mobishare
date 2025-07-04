using System;
using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace Mobishare.Infrastructure.Services.ChatBotAIService.Pulgins;

public class RoutingPagePlugin
{
    [KernelFunction, Description("Vai alla pagina del portafoglio dove l'utente può caricare credito.")]
    public string RouteToWallet() => "/wallet";

    [KernelFunction, Description("Vai alla pagina per prenotare un veicolo.")]
    public string RouteToBooking() => "/vehicles";

    [KernelFunction, Description("Vai alla pagina di supporto o contatti.")]
    public string RouteToSupport() => "/support";

    [KernelFunction, Description("Vai alla home page.")]
    public string RouteToHome() => "/home";
}