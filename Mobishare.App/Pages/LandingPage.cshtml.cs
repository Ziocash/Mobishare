using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mobishare.App.Services;
using Mobishare.Core.Models.UserRelated;
using Mobishare.Core.Models.Vehicles;
using Mobishare.Core.Requests.Vehicles.RideRequests.Commands;
using Mobishare.Core.Requests.Vehicles.VehicleRequests.Commands;
using Mobishare.Core.UiModels;
using Mobishare.Core.VehicleStatus;

namespace Mobishare.App.Pages
{
    public class LandingPageModel : PageModel
    {
        private readonly ILogger<LandingPageModel> _logger;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly UserManager<IdentityUser> _userManager;

        private readonly IGoogleGeocodingService _googleGeocoding;

        public List<RideDisplayInfo> AllRides { get; set; } = [];

        public LandingPageModel(
            ILogger<LandingPageModel> logger,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            IGoogleGeocodingService googleGeocoding,
            UserManager<IdentityUser> userManager
            )
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClientFactory.CreateClient("CityApi");
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _googleGeocoding = googleGeocoding ?? throw new ArgumentNullException(nameof(googleGeocoding));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public async Task<IActionResult> OnPostReserveVehicle(int vehicleId)
        {
            _logger.LogInformation("Prenotazione confermata per veicolo {VehicleId}", vehicleId);

            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                _logger.LogWarning("Authenticated user has null UserId.");
                return new JsonResult(new { success = false, error = "User not found" });
            }

            // Controllo del saldo prima della prenotazione
            var userBalance = await _httpClient.GetFromJsonAsync<Balance>($"api/Balance/{userId}");
            if (userBalance == null)
            {
                _logger.LogWarning("No balance found for user {UserId}", userId);
                return new JsonResult(new { success = false, error = "Balance not found" });
            }

            // Verifica che l'utente abbia almeno 1 euro
            if (userBalance.Credit < 1.0)
            {
                _logger.LogWarning("Insufficient balance for user {UserId}. Required: €1.00, Available: €{Balance}", 
                    userId, userBalance.Credit);
                _logger.LogInformation("Prenotazione bloccata per fondi insufficienti. User: {UserId}, Saldo: €{Balance}", userId, userBalance.Credit);
                return new JsonResult(new { success = false, error = "insufficient_funds", balance = userBalance.Credit });
            }

            _logger.LogInformation("Saldo verificato OK per user {UserId}. Saldo: €{Balance}", userId, userBalance.Credit);

            var vehicle = await _httpClient.GetFromJsonAsync<Vehicle>($"api/Vehicle/{vehicleId}");

            if (vehicle == null)
            {
                _logger.LogWarning("Vehicle with ID {Id} not found", vehicleId);
                return Page();
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
                var errorContent = await updateResponse.Content.ReadAsStringAsync();
                _logger.LogError($"API error: {updateResponse.StatusCode}, Content: {errorContent}");
                return new JsonResult(new { success = false, error = "Failed to update vehicle" });
            }
            else
            {
                _logger.LogInformation("Veicolo {VehicleId} prenotato con successo!", vehicleId);
                return new JsonResult(new { success = true });
            }
        }

        public async Task<IActionResult> OnPostFreeVehicle(int vehicleId)
        {
            _logger.LogInformation("Liberazione confermata per veicolo {VehicleId}", vehicleId);
            var vehicle = await _httpClient.GetFromJsonAsync<Vehicle>($"api/Vehicle/{vehicleId}");

            if (vehicle == null)
            {
                _logger.LogWarning("Vehicle with ID {Id} not found", vehicleId);
                return Page();
            }

            var updateResponse = await _httpClient.PutAsJsonAsync("api/Vehicle",
                new UpdateVehicle
                {
                    Id = vehicle.Id,
                    Plate = vehicle.Plate,
                    Status = VehicleStatusType.Free.ToString(),
                    BatteryLevel = vehicle.BatteryLevel,
                    ParkingSlotId = vehicle.ParkingSlotId,
                    VehicleTypeId = vehicle.VehicleTypeId,
                    CreatedAt = vehicle.CreatedAt
                }
            );

            if (!updateResponse.IsSuccessStatusCode)
            {
                var errorContent = await updateResponse.Content.ReadAsStringAsync();
                _logger.LogError($"API error: {updateResponse.StatusCode}, Content: {errorContent}");
                TempData["ErrorMessage"] = $"Failed to update vehicle. Error: {errorContent}";
            }
            else
            {
                TempData["SuccessMessage"] = $"Veicolo {vehicleId} liberato con successo!";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostStartRide(int vehicleId)
        {
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                _logger.LogWarning("Authenticated user has null UserId.");
                return Page();
            }

            _logger.LogInformation("Inizio corsa confermato per veicolo {VehicleId}", vehicleId);
            var vehicle = await _httpClient.GetFromJsonAsync<Vehicle>($"api/Vehicle/{vehicleId}");

            if (vehicle == null)
            {
                _logger.LogWarning("Vehicle with ID {Id} not found", vehicleId);
                return Page();
            }

            var position = await _httpClient.GetFromJsonAsync<Position>($"api/Position/{vehicleId}");

            var updateResponse = await _httpClient.PutAsJsonAsync("api/Vehicle",
                new UpdateVehicle
                {
                    Id = vehicle.Id,
                    Plate = vehicle.Plate,
                    Status = VehicleStatusType.Occupied.ToString(),
                    BatteryLevel = vehicle.BatteryLevel,
                    ParkingSlotId = vehicle.ParkingSlotId,
                    VehicleTypeId = vehicle.VehicleTypeId,
                    CreatedAt = vehicle.CreatedAt
                }
            );

            if (!updateResponse.IsSuccessStatusCode)
            {
                var errorContent = await updateResponse.Content.ReadAsStringAsync();
                _logger.LogError($"API error: {updateResponse.StatusCode}, Content: {errorContent}");
                TempData["ErrorMessage"] = $"Failed to update vehicle. Error: {errorContent}";
                return Page();
            }

            var createResponse = await _httpClient.PostAsJsonAsync("api/Ride",
                new CreateRide
                {
                    StartDateTime = DateTime.UtcNow,
                    Price = 0, // Set a default price or calculate it based on your logic
                    PositionStartId = position.Id, // This will be set when the ride starts
                    PositionEndId = null, // This will be set when the ride ends
                    VehicleId = vehicleId,
                    UserId = userId
                }
            );

            if (!createResponse.IsSuccessStatusCode)
            {
                var errorContent = await createResponse.Content.ReadAsStringAsync();
                _logger.LogError($"API error: {createResponse.StatusCode}, Content: {errorContent}");
                return new JsonResult(new { success = false, error = "Failed to create ride" });
            }

            // Prenotazione completata con successo
            return RedirectToPage("/Travel");
        }

        public async Task<IActionResult> OnGet()
        {
            if (User?.Identity == null || !User.Identity.IsAuthenticated)
                return RedirectToPage("/Index");

            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                _logger.LogWarning("Authenticated user has null UserId.");
                return RedirectToPage("/Index");
            }

            var ridesResponse = await _httpClient.GetFromJsonAsync<IEnumerable<Ride>>($"api/Ride/AllUserRides/{userId}");
            var rides = ridesResponse ?? [];

            foreach (var ride in rides)
            {
                var rideInfo = new RideDisplayInfo { Ride = ride };

                if (ride.PositionEndId != null)
                {
                    _logger.LogDebug("Location end: lat={Lat}, lon={Lon}", ride.PositionEnd.Latitude, ride.PositionEnd.Longitude);
                    rideInfo.EndLocationName = await _googleGeocoding.GetAddressFromCoordinatesAsync((double)ride.PositionEnd.Latitude, (double)ride.PositionEnd.Longitude);

                    _logger.LogDebug("Address result: {Address}", rideInfo.EndLocationName);
                }
                if (ride.PositionStartId != null) rideInfo.StartLocationName = await _googleGeocoding.GetAddressFromCoordinatesAsync((double)ride.PositionStart.Latitude, (double)ride.PositionStart.Longitude);

                AllRides.Add(rideInfo);
            }

            return Page();
        }
    }
}