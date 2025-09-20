using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Mobishare.Ai.ChatBotAIService.Prompts;
using Mobishare.Core.Models.UserRelated;
using Mobishare.Core.Models.Vehicles;
using Mobishare.Core.ReportStatusEnum;
using Mobishare.Core.Requests.Vehicles.ReportAssignmentsRequests.Commands;
using Mobishare.Core.Requests.Vehicles.ReportRequests.Commands;
using Mobishare.Core.Services.UserContext;


namespace Mobishare.Ai.ChatBotAIService.ToolExecutor.Tools.VehicleTools;

public class VehicleTool : IVehicleTool
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<VehicleTool> _logger;
    private readonly IUserContextService _userContext;
    public string? UserId { get; set; }

    public VehicleTool(
        IHttpClientFactory httpClientFactory,
        // ILogger<VehicleTool> logger,
        IUserContextService userContext,
        UserManager<IdentityUser> userManager)
    {
        _httpClient = httpClientFactory.CreateClient("CityApi");
        // _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
    }
    public VehicleTool() {
         _httpClient = HttpClientContext.HttpClientFactory.CreateClient("CityApi");
     }

    public async Task<string> ReportVehicleIssueAsync(string description, int vehicleId)
    {
        var userId = UserContext.UserId;

        var getTechnicians = await _httpClient.GetFromJsonAsync<IEnumerable<TechnicianReports>>("api/TechnicianApis/GetTechniciansReports");
        
        if (getTechnicians == null)
        {
            return $"Nessun tecnico disponibile per gestire il report: {description}";
        }

        // test per vedere se il veicolo con quell'id esiste
       
        var _chat = HttpClientContext.Chat;

        var promptGenerator = new PromptCollections();
        var promptText = promptGenerator.ReportPrompt(getTechnicians, description);

        var technicianId = "";
        await foreach (var response in _chat.SendAsync(promptText))
        {
            technicianId += response;
        }

        var verifiedTechnician = HttpClientContext.UserManagerController.FindByIdAsync(technicianId).Result;
        if (verifiedTechnician == null)
        {
            return $"Nessun tecnico trovato per gestire il report: {description}";
        }

        var createReport = await _httpClient.PostAsJsonAsync("api/Report",
            new CreateReport
            {
                Description = description,
                UserId = userId,
                VehicleId = vehicleId,
                Status = ReportStatus.Pending.ToString(),
                CreatedAt = DateTime.UtcNow,
            }
        );

        if (!createReport.IsSuccessStatusCode)
        {
            // var errorContent = await createReport.Content.ReadAsStringAsync();
            return $"Errore durante la segnalazione: {description}";
            //_logger.LogError($"API error: {createReport.StatusCode}, Content: {errorContent}");
        }

        var createdReport = await createReport.Content.ReadFromJsonAsync<Report>();
        var reportId = createdReport.Id;
        
        var createReportAssignment = await _httpClient.PostAsJsonAsync("api/Report",
            new CreateReportAssignment
            {
                UserId = technicianId,
                ReportId = reportId
            }
        );

        if (!createReportAssignment.IsSuccessStatusCode)
        {
            return $"Errore durante l'assegnamento della segnalazione: {description}";
        }

        return $"Segnalazione ricevuta con successo!";
    }


    /// <summary>
    /// Reserves a vehicle. UserId, Lat and Lon are set from the UserContextService.
    /// </summary>
    /// <param name="vehicleId"></param>
    /// <returns></returns>
    public async Task<string> ReserveVehicleAsync(string userRequest)
    {
        UserId = UserContext.UserId;
        var latUsr = UserContext.Lat;
        var LonUsr = UserContext.Lon;

        // Debug: controlla se i dati ci sono
        if (string.IsNullOrEmpty(latUsr) || string.IsNullOrEmpty(LonUsr))
        {
            return $"Coordinate utente non disponibili. Lat: {latUsr ?? "null"}, Lon: {LonUsr ?? "null"}";
        }

        var _chat = HttpClientContext.Chat;

        var promptGenerator = new PromptCollections();

        var availableVehicles = await _httpClient.GetFromJsonAsync<IEnumerable<Vehicle>>("api/Vehicle/GetAvailableVehicles");
        if (availableVehicles == null || !availableVehicles.Any())
        {
            return "Nessun veicolo disponibile per la prenotazione.";
        }

        var vehicleList = new List<(int VehicleId, decimal Latitude, decimal Longitude)>();

        foreach (var vehicle in availableVehicles)
        {
            // Recupera la posizione solo per il veicolo corrente
            var pos = await _httpClient.GetFromJsonAsync<Position>($"api/Position/{vehicle.Id}");
            vehicleList.Add((vehicle.Id, pos?.Latitude ?? 0, pos?.Longitude ?? 0));
        }


        var promptText = promptGenerator.ReservationPrompt(userRequest, latUsr + "," + LonUsr, vehicleList);

        var vehicleId = "";
        await foreach (var res in _chat.SendAsync(promptText))
        {
            vehicleId += res;
        }

        //var responseFromApi = await _httpClient.PostAsJsonAsync("/LandingPage?handler=ReserveVehicle", new { vehicleId = vehicleId });
        // if (!int.TryParse(vehicleId.Trim(), out int parsedVehicleId))
        // {
        //     return "Errore: ID veicolo non valido ricevuto dall'AI.";
        // }

        // var response = await _httpClient.PostAsJsonAsync("api/Vehicle/Reserve", new { 
        //     VehicleId = parsedVehicleId,
        //     UserId = _userContext.UserId 
        // });

        // if (!response.IsSuccessStatusCode)
        // {
        //     return $"Errore durante la prenotazione del veicolo. Status: {response.StatusCode}";
        // }
        
        if (!int.TryParse(vehicleId.Trim(), out int parsedVehicleId))
        {
            return "Errore: ID veicolo non valido ricevuto dall'AI.";
        }

        var formContent = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("vehicleId", parsedVehicleId.ToString())
        });

        var response = await _httpClient.PostAsync("/LandingPage?handler=ReserveVehicle", formContent);

        if (!response.IsSuccessStatusCode)
        {
            return $"Errore durante la prenotazione del veicolo. Status: {response.StatusCode}";
        }

        var responseContent = await response.Content.ReadAsStringAsync();

        

        return "Veicolo riservato con successo.";
    }
}
