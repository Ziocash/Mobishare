using Microsoft.SemanticKernel;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Mobishare.Infrastructure.Services;

public class SemanticRouterService
{
    private readonly Kernel _kernel;
    private readonly ILogger<SemanticRouterService> _logger;

    public SemanticRouterService(Kernel kernel, ILogger<SemanticRouterService> logger)
    {
        _kernel = kernel;
        _logger = logger;
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

        try
        {
            var result = await _kernel.InvokePromptAsync(prompt);
            var functionName = result.ToString()?.Trim();

            if (string.IsNullOrEmpty(functionName))
            {
                _logger.LogWarning("Nessuna funzione di routing identificata per: {UserInput}", userInput);
                return "/home";
            }

            _logger.LogInformation("Funzione selezionata: {FunctionName} per: {UserInput}", functionName, userInput);

            // Correzione: accesso corretto al plugin
            if (!_kernel.Plugins.TryGetPlugin("Routing", out var plugin))
            {
                _logger.LogError("Plugin 'Routing' non trovato");
                return "/home";
            }

            // Correzione: invocazione corretta della funzione
            if (plugin.TryGetFunction(functionName, out var function))
            {
                var context = new KernelArguments();
                var resultValue = await function.InvokeAsync(_kernel, context);
                return resultValue.GetValue<string>() ?? "/home";
            }
            else
            {
                _logger.LogWarning("Funzione {FunctionName} non trovata nel plugin", functionName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore nel routing semantico per: {UserInput}", userInput);
        }
        
        return "/home";
    }
}