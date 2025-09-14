using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mobishare.App.Services;
using Mobishare.Core.Data;
using Mobishare.Core.Models.Vehicles;
using Mobishare.Core.Models.UserRelated;
using Mobishare.Core.Requests.Vehicles.RideRequests.Commands;
using Mobishare.Core.Requests.Vehicles.VehicleRequests.Commands;
using Mobishare.Core.Requests.Users.BalanceRequest.Commands;
using Mobishare.Core.Requests.Users.HistoryCreditRequest.Commands;
using Mobishare.Core.Enums.Balance;
using Mobishare.Core.VehicleStatus;
using Microsoft.EntityFrameworkCore;
using Mobishare.Core.Models.Maps;
using NetTopologySuite.IO;

namespace Mobishare.App.Pages
{

    public class TravelModel : PageModel
    {
        private readonly ILogger<LandingPageModel> _logger;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IGoogleGeocodingService _googleGeocoding;
        private readonly ApplicationDbContext _context;

        public IEnumerable<ParkingSlot> AllParkingSlots { get; set; }
        public IEnumerable<City> AllCities { get; set; }
        public string AllParkingSlotsPerimeter { get; set; }
        public string AllCitiesPerimeter { get; set; }
        public String? StartLocationName;
        public Ride? Ride;
        public int VehicleId;
        public VehicleType? VehicleType;
        public Position? StartPosition;
        public decimal CostPerMinute { get; set; }

        public TravelModel(
            ILogger<LandingPageModel> logger,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            IGoogleGeocodingService googleGeocoding,
            UserManager<IdentityUser> userManager,
            ApplicationDbContext context
            )
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClientFactory.CreateClient("CityApi");
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _googleGeocoding = googleGeocoding ?? throw new ArgumentNullException(nameof(googleGeocoding));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _context = context;
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

            Ride = await _httpClient.GetFromJsonAsync<Ride>($"api/Ride/User/{userId}");

            if (Ride == null)
            {
                _logger.LogInformation("No active ride found for user with ID: {UserId}. Redirecting to landing page.", userId);
                return RedirectToPage("/LandingPage");
            }

            // Recupera il veicolo con il tipo per ottenere il costo per minuto
            var Vehicle = await _httpClient.GetFromJsonAsync<Vehicle>($"api/Vehicle/{Ride.VehicleId}");
            if (Vehicle != null)
            {
                VehicleType = await _httpClient.GetFromJsonAsync<VehicleType>($"api/VehicleType/{Vehicle.VehicleTypeId}");
                if (VehicleType != null)
                {
                    CostPerMinute = VehicleType.PricePerMinute;
                }
            }

            StartPosition = await _httpClient.GetFromJsonAsync<Position>($"api/Position/{Ride.VehicleId}");

            if (StartPosition != null)
                StartLocationName = await _googleGeocoding.GetAddressFromCoordinatesAsync((double)StartPosition.Latitude, (double)StartPosition.Longitude);

            try
            {
                var response = await _httpClient.GetFromJsonAsync<IEnumerable<City>>("api/City/AllCities");
                AllCities = response ?? [];
                var parkigSlotResponse = await _httpClient.GetFromJsonAsync<IEnumerable<ParkingSlot>>("api/ParkingSlot/AllAvailableParkingSlots");
                AllParkingSlots = parkigSlotResponse ?? [];

                AllCitiesPerimeter = string.Join(";", AllCities.Select(c => c.PerimeterLocation));
                AllParkingSlotsPerimeter = string.Join(";", AllParkingSlots.Select(p => p.PerimeterLocation));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading data");
            }

            return Page();
        }

        //[HttpGet]
        public async Task<IActionResult> OnGetCurrentCostAsync(int rideId)
        {
            var ride = await _context.Rides
                .Include(r => r.Vehicle)
                .ThenInclude(v => v.VehicleType)
                .FirstOrDefaultAsync(r => r.Id == rideId);

            if (ride == null)
            {
                return NotFound();
            }

            var elapsedMinutes = (DateTime.UtcNow - ride.StartDateTime).TotalMinutes;
            var currentCost = Math.Round((decimal)elapsedMinutes * ride.Vehicle.VehicleType.PricePerMinute, 2);

            return new JsonResult(new { cost = currentCost });
        }

        public async Task<IActionResult> OnPostEndTrip(string tripName, int rideId)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (userId == null)
                {
                    _logger.LogWarning("Authenticated user has null UserId.");
                    return RedirectToPage("/Index");
                }

                // Recupera il ride
                var ride = await _httpClient.GetFromJsonAsync<Ride>($"api/Ride/{rideId}");
                if (ride == null)
                {
                    _logger.LogWarning("Ride with ID {RideId} not found", rideId);
                    return RedirectToPage("/Index");
                }

                // Recupera il veicolo per ottenere il tipo
                var vehicle = await _httpClient.GetFromJsonAsync<Vehicle>($"api/Vehicle/{ride.VehicleId}");
                if (vehicle == null)
                {
                    _logger.LogWarning("Vehicle with ID {VehicleId} not found", ride.VehicleId);
                    return RedirectToPage("/Index");
                }
                // Recupera l'ultima posizione del veicolo
                var lastPosition = await _httpClient.GetFromJsonAsync<Position>($"api/Position/{ride.VehicleId}");

                var isInsideParkingSlot = false;
                var reader = new WKTReader();
                var currentPoint = new NetTopologySuite.Geometries.Point(
                    (double)lastPosition.Longitude,
                    (double)lastPosition.Latitude
                );

                var parkingSlotResponse = await _httpClient.GetFromJsonAsync<IEnumerable<ParkingSlot>>("api/ParkingSlot/AllAvailableParkingSlots");
                AllParkingSlots = parkingSlotResponse ?? [];

                foreach (var parkingSlot in AllParkingSlots)
                {
                    if (!string.IsNullOrEmpty(parkingSlot.PerimeterLocation))
                    {
                        var polygon = (NetTopologySuite.Geometries.Polygon)reader.Read(parkingSlot.PerimeterLocation);

                        if (polygon.Contains(currentPoint))
                        {
                            isInsideParkingSlot = true;
                            _logger.LogInformation("Vehicle for ride {RideId} is inside parking slot {ParkingSlotId}", rideId, parkingSlot.Id);
                            break;
                        }
                    }
                }

                if (!isInsideParkingSlot)
                {
                    TempData["ErrorMessage"] = "Please, go into a parking slot.";
                    return Page();
                }

                // Recupera il tipo di veicolo per ottenere il prezzo al minuto
                var vehicleType = await _httpClient.GetFromJsonAsync<VehicleType>($"api/VehicleType/{vehicle.VehicleTypeId}");
                if (vehicleType == null)
                {
                    _logger.LogWarning("VehicleType with ID {VehicleTypeId} not found", vehicle.VehicleTypeId);
                    return RedirectToPage("/Index");
                }

                // Calcola la durata in minuti e il prezzo
                var endTime = DateTime.UtcNow;
                var durationInMinutes = (endTime - ride.StartDateTime).TotalMinutes;
                var calculatedPrice = 0m;
                if (durationInMinutes < 30)
                    calculatedPrice = (decimal)(FixedCostVehicle)Enum.Parse(typeof(FixedCostVehicle), vehicleType.Type);

                else
                    calculatedPrice = Math.Round(((decimal)durationInMinutes - 30) * vehicleType.PricePerMinute, 2);


                // Recupera il saldo dell'utente
                var userBalance = await _httpClient.GetFromJsonAsync<Balance>($"api/Balance/{userId}");
                if (userBalance == null)
                {
                    _logger.LogWarning("No balance found for user {UserId}", userId);
                    TempData["ErrorMessage"] = "Impossibile recuperare il saldo dell'utente.";
                    return Page();
                }

                // Verifica che l'utente abbia fondi sufficienti
                if (userBalance.Credit < (double)calculatedPrice)
                {
                    _logger.LogWarning("Insufficient balance for user {UserId}. Required: €{Price}, Available: €{Balance}",
                        userId, calculatedPrice, userBalance.Credit);
                    TempData["ErrorMessage"] = $"Saldo insufficiente. Necessari: €{calculatedPrice:F2}, Disponibili: €{userBalance.Credit:F2}. Ricarica il tuo wallet.";
                    return Page();
                }

                // Scala il costo dal saldo dell'utente
                var newBalance = userBalance.Credit - (double)calculatedPrice;
                var updateBalanceResponse = await _httpClient.PutAsJsonAsync("api/Balance",
                    new UpdateBalance
                    {
                        Id = userBalance.Id,
                        Credit = newBalance,
                        Points = userBalance.Points,
                        UserId = userId
                    }
                );

                if (!updateBalanceResponse.IsSuccessStatusCode)
                {
                    var errorContent = await updateBalanceResponse.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to update balance: {updateBalanceResponse.StatusCode}, Content: {errorContent}");
                    TempData["ErrorMessage"] = "Errore nell'aggiornamento del saldo.";
                    return Page();
                }

                // Crea una voce nello storico delle transazioni
                var createHistoryResponse = await _httpClient.PostAsJsonAsync("api/HistoryCredit",
                    new CreateHistoryCredit
                    {
                        UserId = userId,
                        Credit = (double)calculatedPrice,
                        TransactionType = CreditTransactionType.RidePayment.ToString(),
                        CreatedAt = DateTime.UtcNow,
                        BalanceId = userBalance.Id
                    }
                );

                if (!createHistoryResponse.IsSuccessStatusCode)
                {
                    var errorContent = await createHistoryResponse.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to create history credit: {createHistoryResponse.StatusCode}, Content: {errorContent}");
                    // Non blocchiamo il processo per questo errore, ma lo logghiamo
                }




                // Aggiorna il ride con orario di fine, posizione finale e prezzo calcolato
                var updateRideResponse = await _httpClient.PutAsJsonAsync("api/Ride",
                    new UpdateRide
                    {
                        Id = ride.Id,
                        StartDateTime = ride.StartDateTime,
                        EndDateTime = endTime,
                        Price = (double)calculatedPrice, // Prezzo calcolato basato sulla durata
                        PositionStartId = ride.PositionStartId,
                        PositionEndId = lastPosition?.Id,
                        UserId = ride.UserId,
                        VehicleId = ride.VehicleId,
                        TripName = string.IsNullOrWhiteSpace(tripName) ? null : tripName
                    }
                );

                if (!updateRideResponse.IsSuccessStatusCode)
                {
                    var errorContent = await updateRideResponse.Content.ReadAsStringAsync();
                    _logger.LogError($"API error: {updateRideResponse.StatusCode}, Content: {errorContent}");
                    TempData["ErrorMessage"] = $"Failed to update Ride. Error: {errorContent}";
                }

                // Libera il veicolo (rimetti a Free)
                var updateVehicleResponse = await _httpClient.PutAsJsonAsync("api/Vehicle", new UpdateVehicle
                {
                    Id = vehicle.Id,
                    Plate = vehicle.Plate,
                    Status = VehicleStatusType.Free.ToString(),
                    BatteryLevel = vehicle.BatteryLevel,
                    ParkingSlotId = vehicle.ParkingSlotId,
                    VehicleTypeId = vehicle.VehicleTypeId,
                    CreatedAt = vehicle.CreatedAt
                });

                if (!updateVehicleResponse.IsSuccessStatusCode)
                {
                    var errorContent = await updateVehicleResponse.Content.ReadAsStringAsync();
                    _logger.LogError($"API error: {updateVehicleResponse.StatusCode}, Content: {errorContent}");
                    TempData["ErrorMessage"] = $"Failed to update vehicle type. Error: {errorContent}";
                    return Page();
                }

                _logger.LogInformation("Trip ended successfully for ride {RideId}. Duration: {Duration} minutes, Price: €{Price}, Balance deducted: €{Deducted}",
                    rideId, Math.Round(durationInMinutes, 2), calculatedPrice, calculatedPrice);

                TempData["SuccessMessage"] = $"Viaggio terminato con successo! Durata: {Math.Round(durationInMinutes, 0)} minuti - Costo: €{calculatedPrice:F2} - Nuovo saldo: €{newBalance:F2}";

                return RedirectToPage("/LandingPage");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ending trip for ride {RideId}", rideId);
                TempData["ErrorMessage"] = "Errore nel terminare il viaggio.";
                return Page();
            }
        }
    }
}
