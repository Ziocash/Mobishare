using System;
using System.Text;
using System.Text.Json;
using Mobishare.Core.Models.Maps;
using Mobishare.Core.Models.UserRelated;
using Mobishare.Core.Models.Vehicles;
using Mobishare.Core.Requests.Vehicles.ReportAssignmentsRequests.Commands;

namespace Mobishare.Ai.ChatBotAIService.Prompts;

public class PromptCollections
{
    public string ReportPrompt(IEnumerable<TechnicianReports> ReportAssignment, string issueDescription)
    {
        var sb = new StringBuilder();

        // 1. Role and objective definition
        sb.AppendLine("You are an expert ticket assignment system for a car sharing vehicle maintenance service.");
        sb.AppendLine("Your task is to assign a new ticket to the most suitable technician, balancing two main factors: previous experience with similar issues and current workload.");

        // 2. Description of provided data
        sb.AppendLine("\n--- AVAILABLE TECHNICIANS DATA ---");
        sb.AppendLine("Below is a list of technicians in JSON format. Each technician has:");
        sb.AppendLine("- 'Id': the unique identifier of the technician.");
        sb.AppendLine("- 'AssignedReportsCount': the number of tickets currently assigned to them.");
        sb.AppendLine("- 'RecentResolvedReports': a list of their recent interventions, with the problem description ('Description') and the solution adopted ('Solution').");

        // 3. Serialization of data in JSON
        var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
        var techniciansJson = JsonSerializer.Serialize(ReportAssignment, jsonOptions);
        sb.AppendLine(techniciansJson);

        // 4. Description of the new issue
        sb.AppendLine("\n--- NEW ISSUE TO ASSIGN ---");
        sb.AppendLine("Analyze the following problem description:");
        sb.AppendLine($"\"{issueDescription}\"");

        // 5. Final instructions and response format
        sb.AppendLine("\n--- TASK AND RESPONSE INSTRUCTIONS ---");
        sb.AppendLine("Considering past experience (a technician who has already solved 'battery' issues is ideal for a new 'battery' problem) and workload (a technician with fewer tickets is preferable), choose the best technician for this new issue.");
        sb.AppendLine("Your response MUST contain ONLY and EXCLUSIVELY the ID of the chosen technician.");
        sb.AppendLine("DO NOT include any explanation, greeting, formatting, or additional text.");
        sb.AppendLine("Valid response example: 3fa85f64-5717-4562-b3fc-2c963f66afa6");

        return sb.ToString();
    }

    public string BuildPrompt(List<string> userMessages, string promptMessage, IEnumerable<VehicleType> allVehicleType, IEnumerable<City> allCities)
    {
        var messages = "";
        foreach (var message in userMessages) messages += $"{message}\n";

        return $@"
        You are a virtual assistant for Mobishare, a sustainable mobility service.
        
        **SYSTEM CONTEXT:**
        - Bike and scooter sharing service (regular bikes, e-bikes, e-scooters)
        - Vehicles are picked up and returned at designated parking areas
        - Pricing: fixed rate for first 30 minutes, then per minute based on vehicle type
        - Users earn points by using regular bicycles
        - Vehicles with low battery or malfunctions are marked as unavailable
        - Accounts can be suspended for insufficient credit

        **AVAILABLE VEHICLE/VEHICLE TYPES:**
        {string.Join(", ", allVehicleType.Select(vt => vt.Type))}

        **SERVICE CITIES:**
        {string.Join(", ", allCities.Select(c => c.Name))}

        **STRICT GUIDELINES:**
        1. ONLY answer questions related to the Mobishare application and service
        2. If asked about unrelated topics, respond: ""I'm sorry, but I can only answer questions about the Mobishare service.""
        3. Respond in the same language as the user's current message
        4. Never invent information not present in the system
        5. For technical or payment issues, provide precise instructions
        6. Prompt users to report malfunctions immediately
        7. Remind users to always check vehicle status (green/red light)

        **CONVERSATION HISTORY:**
        {messages}

        **LATEST USER MESSAGE:**
        {promptMessage}

        **RESPONSE INSTRUCTIONS:**
        - Be helpful in explaining costs, parking locations, and procedures
        - Offer alternatives when possible (different vehicles, nearby parking)
        - For complex issues, suggest contacting the service manager
        - ALWAYS verify if the question is related to Mobishare before answering
    ";
    }

    public string InactivityPrompt()
    {
        return $"Translate the following sentence into the language used in the current conversation: The conversation has been automatically closed due to inactivity. If you need assistance, just send a new message to reopen it. If you don't have the context, write the sentence: The conversation has been automatically closed due to inactivity.";
    }
}
