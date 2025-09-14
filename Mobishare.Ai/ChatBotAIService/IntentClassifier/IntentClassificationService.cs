using System;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Logging;
using Mobishare.Core.Enums.Ai;

namespace Mobishare.Ai.ChatBotAIService.IntentClassifier;

public class IntentClassificationService : IIntentClassificationService
{
    private readonly IOllamaService _ollamaService;
    private readonly ILogger<IntentClassificationService> _logger;

    public IntentClassificationService(IOllamaService ollamaService, ILogger<IntentClassificationService> logger)
    {
        _ollamaService = ollamaService ?? throw new ArgumentNullException(nameof(ollamaService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> ClassifyMessageAsync(string userMessage)
    {
        var sb = new StringBuilder();

        sb.AppendLine("You are an assistant that decides if the user's message requires calling a tool or answering directly.");
        sb.AppendLine("Available tools and their descriptions:");

        foreach (ToolsClassification tool in Enum.GetValues(typeof(ToolsClassification)))
        {
            var fieldInfo = tool.GetType().GetField(tool.ToString());
            ToolInfoAttribute? attribute = null;
            if (fieldInfo != null) attribute = fieldInfo.GetCustomAttribute(typeof(ToolInfoAttribute)) as ToolInfoAttribute;

            if (attribute != null) sb.AppendLine($"{tool}: {attribute.Description}");
        }

        sb.AppendLine();
        sb.AppendLine($"Given the user's message: \"{userMessage}\"");
        sb.AppendLine();
        sb.AppendLine("Answer only with one of these values:");
        sb.AppendLine("- the tool name that should be used (e.g. book_vehicle)");
        sb.AppendLine("- or \"none\" if the assistant can answer directly.");

        string prompt = sb.ToString();

        var response = await _ollamaService.GetResponseAsync(prompt);
        _logger.LogInformation("Retrive response: {response}", response);

        return response.Trim().ToLower();
    }
}
