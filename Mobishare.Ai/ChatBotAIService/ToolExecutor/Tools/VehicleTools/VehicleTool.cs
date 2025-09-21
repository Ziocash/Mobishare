using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Mobishare.Ai.ChatBotAIService.Prompts;
using Mobishare.Core.Models.UserRelated;
using Mobishare.Core.Models.Vehicles;
using Mobishare.Core.ReportStatusEnum;
using Mobishare.Core.Requests.Vehicles.ReportAssignmentsRequests.Commands;
using Mobishare.Core.Requests.Vehicles.ReportRequests.Commands;
using Mobishare.Core.Requests.Vehicles.VehicleRequests.Commands;
using Mobishare.Core.Services.UserContext;
using Mobishare.Core.VehicleStatus;


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

    /// <summary>
    /// Reports a vehicle issue. UserId is set from the UserContextService.
    /// </summary> <param name="description"></param> <param name="vehicleId"></param>
    /// <returns></returns>
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

        foreach (var veh in availableVehicles)
        {
            // Recupera la posizione solo per il veicolo corrente
            var pos = await _httpClient.GetFromJsonAsync<Position>($"api/Position/{veh.Id}");
            vehicleList.Add((veh.Id, pos?.Latitude ?? 0, pos?.Longitude ?? 0));
        }

        var promptText = promptGenerator.ReservationPrompt(userRequest, latUsr + "," + LonUsr, vehicleList);

        var vehicleId = "";

        // Estrai l'ID del veicolo dalla risposta dell'AI
        await foreach (var res in _chat.SendAsync(promptText))
        {
            vehicleId += res;
        }

        
        if (!int.TryParse(vehicleId.Trim(), out int parsedVehicleId))
        {
            return "Errore: ID veicolo non valido ricevuto dall'AI.";
        }
        // Controlla se l'utente ha un saldo sufficiente
        var userBalance = await _httpClient.GetFromJsonAsync<Balance>($"api/Balance/{UserId}");
        if (userBalance == null)
        {
            //_logger.LogWarning("No balance found for user {UserId}", UserId);
            return "Errore: non è stato trovato nessun wallet.";
        }

        // Verifica che l'utente abbia almeno 1 euro
        if (userBalance.Credit < 1.0)
        {

            return "Saldo insufficiente per la prenotazione. Si prega di ricaricare il conto.";
        }

        // Effettua la prenotazione del veicolo
        var vehicle = await _httpClient.GetFromJsonAsync<Vehicle>($"api/Vehicle/{parsedVehicleId}");

        if (vehicle == null)
        {

            return $"ERRORE: veicolo con ID {parsedVehicleId} inesistente.";
        }

        var updateResponse = await _httpClient.PutAsJsonAsync("api/Vehicle",
            new UpdateVehicle
            {
                Id = vehicle.Id,
                Plate = vehicle.Plate,
                Status = VehicleStatusType.Reserved.ToString(),
                BatteryLevel = vehicle.BatteryLevel,
                ParkingSlotId = vehicle.ParkingSlotId,
                VehicleTypeId = vehicle.VehicleTypeId,
                CreatedAt = vehicle.CreatedAt
            }
        );

        if (!updateResponse.IsSuccessStatusCode)
        {
            return $"Errore durante la prenotazione del veicolo. Status: {updateResponse.StatusCode}";
        }
        else
        {
            return "Veicolo prenotato con successo.";
        }

    }
}
