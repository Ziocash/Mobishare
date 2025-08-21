using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mobishare.App.Services;
using Mobishare.Core.Data;
using Mobishare.Core.Models.Vehicles;
using Mobishare.Core.Requests.Vehicles.PositionRequests.Queries;
using Mobishare.Core.Requests.Vehicles.RideRequests.Commands;
using Mobishare.Core.Requests.Vehicles.RideRequests.Queries;
using Mobishare.Core.Requests.Vehicles.VehicleRequests.Commands;
using Mobishare.Core.Requests.Vehicles.VehicleRequests.Queries;
using Mobishare.Core.Requests.Vehicles.VehicleTypeRequests.Queries;
using Mobishare.Core.UiModels;
using Mobishare.Core.VehicleStatus;
using Microsoft.EntityFrameworkCore;

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

        public String? StartLocationName;
        public Ride Ride;
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

            Ride = await _httpClient.GetFromJsonAsync<Ride>($"api/Ride/{userId}");

            if (Ride == null)
            {
                _logger.LogWarning("No ride found for user with ID: {UserId}", userId);
                return RedirectToPage("/Index");
            }

            // Recupera il veicolo con il tipo per ottenere il costo per minuto
            var vehicle = await _mediator.Send(new GetVehicleById(Ride.VehicleId));
            if (vehicle != null)
            {
                var vehicleType = await _mediator.Send(new GetVehicleTypeById(vehicle.VehicleTypeId));
                if (vehicleType != null)
                {
                    CostPerMinute = vehicleType.PricePerMinute;
                }
            }

            StartPosition = await _mediator.Send(new GetPositionByVehicleId(Ride.VehicleId));

            if (StartPosition != null)
                StartLocationName = await _googleGeocoding.GetAddressFromCoordinatesAsync((double)StartPosition.Latitude, (double)StartPosition.Longitude);
         
            return Page();
        }

        [HttpGet]
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
                var ride = await _mediator.Send(new GetRideById(rideId));
                if (ride == null)
                {
                    _logger.LogWarning("Ride with ID {RideId} not found", rideId);
                    return RedirectToPage("/Index");
                }

                // Recupera il veicolo per ottenere il tipo
                var vehicle = await _mediator.Send(new GetVehicleById(ride.VehicleId));
                if (vehicle == null)
                {
                    _logger.LogWarning("Vehicle with ID {VehicleId} not found", ride.VehicleId);
                    return RedirectToPage("/Index");
                }

                // Recupera il tipo di veicolo per ottenere il prezzo al minuto
                var vehicleType = await _mediator.Send(new GetVehicleTypeById(vehicle.VehicleTypeId));
                if (vehicleType == null)
                {
                    _logger.LogWarning("VehicleType with ID {VehicleTypeId} not found", vehicle.VehicleTypeId);
                    return RedirectToPage("/Index");
                }

                // Calcola la durata in minuti e il prezzo
                var endTime = DateTime.UtcNow;
                var durationInMinutes = (endTime - ride.StartDateTime).TotalMinutes;
                var calculatedPrice = Math.Round((decimal)durationInMinutes * vehicleType.PricePerMinute, 2);

                // Recupera l'ultima posizione del veicolo
                var lastPosition = await _mediator.Send(new GetPositionByVehicleId(ride.VehicleId));
                
                // Aggiorna il ride con orario di fine, posizione finale e prezzo calcolato
                await _mediator.Send(new UpdateRide
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
                });

                // Libera il veicolo (rimetti a Free)
                await _mediator.Send(new UpdateVehicle
                {
                    Id = vehicle.Id,
                    Plate = vehicle.Plate,
                    Status = VehicleStatusType.Free.ToString(),
                    BatteryLevel = vehicle.BatteryLevel,
                    ParkingSlotId = vehicle.ParkingSlotId,
                    VehicleTypeId = vehicle.VehicleTypeId,
                    CreatedAt = vehicle.CreatedAt
                });

                _logger.LogInformation("Trip ended successfully for ride {RideId}. Duration: {Duration} minutes, Price: €{Price}", 
                    rideId, Math.Round(durationInMinutes, 2), calculatedPrice);
                
                TempData["SuccessMessage"] = $"Viaggio terminato con successo! Durata: {Math.Round(durationInMinutes, 0)} minuti - Costo: €{calculatedPrice}";
                
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
