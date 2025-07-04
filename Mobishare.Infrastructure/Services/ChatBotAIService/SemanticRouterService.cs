using System;
using Microsoft.SemanticKernel;

namespace Mobishare.Infrastructure.Services;

public class SemanticRouterService
{
    private readonly Kernel _kernel;

    public SemanticRouterService(Kernel kernel)
    {
        _kernel = kernel;
    }

    public async Task<string> RouteFromUserInputAsync(string userInput)
    {
        string prompt = $@"
            Dato un messaggio utente, rispondi solo con il nome della funzione da eseguire tra:
            - RouteToWallet
            - RouteToBooking
            - RouteToSupport
            - RouteToHome

            Messaggio utente: {userInput}
            Funzione da chiamare:";

        var result = await _kernel.InvokePromptAsync(prompt);

        var functionName = result?.ToString()?.Trim();

        var plugin = _kernel.Plugins["Routing"];

        if (functionName != null && plugin.TryGetFunction(functionName, out var skFunction))
        {
            var routeResult = await skFunction.InvokeAsync(_kernel);
            return routeResult.ToString() ?? "/home";
        }

        return "/home";
    }
}
