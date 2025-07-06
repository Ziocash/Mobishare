using Microsoft.SemanticKernel;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

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
        try
        {
  // 1. Prepara il prompt
            var prompt = $@"
                ANALISI INTENZIONE UTENTE:
                - Se l'utente chiede di portafoglio, carte o pagamenti: RouteToWallet
                - Se l'utente chiede di veicoli, noleggio o parcheggio: RouteToBooking
                - Se l'utente chiede assistenza o problemi: RouteToSupport
                - Per tutto il resto: RouteToHome
                
                INPUT UTENTE: '{userInput}'
                AZIONE:";
 // 2. Ottieni il servizio di chat
            var chatService = _kernel.GetRequiredService<IChatCompletionService>();
            
            // 3. Prepara la history dei messaggi
            var chatHistory = new ChatHistory();
            chatHistory.AddUserMessage(prompt);
            
            // 4. Esegui la richiesta
            var result = await chatService.GetChatMessageContentsAsync(
                chatHistory,
                executionSettings: new OpenAIPromptExecutionSettings()
                {
                    MaxTokens = 20,
                    Temperature = 0.3
                },
                kernel: _kernel
            );
            
            // 5. Estrai la risposta
            var action = result[0].Content?.Trim() ?? "RouteToHome";
            _logger.LogInformation($"Azione selezionata: {action} per input: {userInput}");
 // 6. Esegui la funzione corrispondente
            var routeFunction = _kernel.Plugins["Routing"][action];
            var arguments = new KernelArguments();
            var routeResult = await routeFunction.InvokeAsync(_kernel, arguments);
            
            return routeResult.GetValue<string>() ?? "/home";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore nel routing semantico per: {UserInput}", userInput);
            return "/home";
        }
   }
}