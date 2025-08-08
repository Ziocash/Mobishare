using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mobishare.App.Services;
using Mobishare.Core.Models.Vehicles;
using Mobishare.Core.Requests.Vehicles.PositionRequests.Queries;
using Mobishare.Core.Requests.Vehicles.RideRequests.Commands;
using Mobishare.Core.Requests.Vehicles.RideRequests.Queries;
using Mobishare.Core.Requests.Vehicles.VehicleRequests.Commands;
using Mobishare.Core.Requests.Vehicles.VehicleRequests.Queries;
using Mobishare.Core.UiModels;
using Mobishare.Core.VehicleStatus;

namespace Mobishare.App.Pages
{

    public class TravelModel : PageModel
    {
        private readonly ILogger<LandingPageModel> _logger;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IGoogleGeocodingService _googleGeocoding;
        public String? StartLocationName;
        public Ride Ride;
        public Position? StartPosition;

        public TravelModel(
            ILogger<LandingPageModel> logger,
            IMediator mediator,
            IMapper mapper,
            IConfiguration configuration,
            IGoogleGeocodingService googleGeocoding,
            UserManager<IdentityUser> userManager
            )
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _googleGeocoding = googleGeocoding ?? throw new ArgumentNullException(nameof(googleGeocoding));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
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

            Ride = await _mediator.Send(new GetRideByUserId(userId));

            if (Ride == null)
            {
                _logger.LogWarning("No ride found for user with ID: {UserId}", userId);
                return RedirectToPage("/Index");
            }

            StartPosition = await _mediator.Send(new GetPositionByVehicleId(Ride.VehicleId));

            if (StartPosition != null)
                StartLocationName = await _googleGeocoding.GetAddressFromCoordinatesAsync((double)StartPosition.Latitude, (double)StartPosition.Longitude);
            return Page();
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

                // Recupera l'ultima posizione del veicolo
                var lastPosition = await _mediator.Send(new GetPositionByVehicleId(ride.VehicleId));
                
                // Aggiorna il ride con orario di fine e posizione finale
                await _mediator.Send(new UpdateRide
                {
                    Id = ride.Id,
                    StartDateTime = ride.StartDateTime,
                    EndDateTime = DateTime.UtcNow,
                    Price = ride.Price, // Calcola il prezzo se necessario
                    PositionStartId = ride.PositionStartId,
                    PositionEndId = lastPosition?.Id,
                    UserId = ride.UserId,
                    VehicleId = ride.VehicleId,
                    TripName = string.IsNullOrWhiteSpace(tripName) ? null : tripName // Se hai questo campo
                });

                // Libera il veicolo (rimetti a Free)
                var vehicle = await _mediator.Send(new GetVehicleById(ride.VehicleId));
                if (vehicle != null)
                {
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
                }

                _logger.LogInformation("Trip ended successfully for ride {RideId}", rideId);
                TempData["SuccessMessage"] = "Viaggio terminato con successo!";
                
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
