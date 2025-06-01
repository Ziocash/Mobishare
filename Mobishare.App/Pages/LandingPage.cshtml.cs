using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mobishare.App.Services;
using Mobishare.Core.Requests.Maps.CityRequests.Commands;
using Mobishare.Core.Requests.Vehicles.RideRequests.Queries;
using Mobishare.Core.Requests.Vehicles.VehicleRequests.Queries;
using Mobishare.Core.UiModels;
using Mobishare.Core.VehicleStatus;

namespace Mobishare.App.Pages
{
    public class LandingPageModel : PageModel
    {
        private readonly ILogger<LandingPageModel> _logger;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly UserManager<IdentityUser> _userManager;

        private readonly IGoogleGeocodingService _googleGeocoding;

        public List<RideDisplayInfo> AllRides { get; set; } = [];

        public LandingPageModel(
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


        public async Task<IActionResult> OnPostReserveVehicle(int vehicleId)
        {
            _logger.LogInformation("Prenotazione confermata per veicolo {VehicleId}", vehicleId);
            var vehicle = await _mediator.Send(new GetVehicleById(vehicleId));
            if (vehicle == null)
            {
                _logger.LogWarning("Vehicle with ID {Id} not found", vehicleId);
                return Page();
            }
            await _mediator.Send(new UpdateVehicle{
                Id = vehicle.Id,
                Plate = vehicle.Plate,
                Status = VehicleStatusType.Reserved.ToString(),
                BatteryLevel = vehicle.BatteryLevel,
                ParkingSlotId = vehicle.ParkingSlotId,
                VehicleTypeId = vehicle.VehicleTypeId,
                CreatedAt = vehicle.CreatedAt
            });



            TempData["SuccessMessage"] = $"Veicolo {vehicleId} prenotato con successo!"; 
            return Page();
        }
        
        public async Task<IActionResult> OnPostFreeVehicle(int vehicleId)
        {
            _logger.LogInformation("Liberazione confermata per veicolo {VehicleId}", vehicleId);
            var vehicle = await _mediator.Send(new GetVehicleById(vehicleId));
            if (vehicle == null)
            {
                _logger.LogWarning("Vehicle with ID {Id} not found", vehicleId);
                return Page();
            }
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

            TempData["SuccessMessage"] = $"Veicolo {vehicleId} liberato con successo!";
            return RedirectToPage();
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

            var rides = await _mediator.Send(new GetAllUserRides(userId));

            foreach (var ride in rides)
            {
                var rideInfo = new RideDisplayInfo { Ride = ride };

                if (ride.PositionEnd != null)
                {
                    _logger.LogDebug("Location end: lat={Lat}, lon={Lon}", ride.PositionEnd.Latitude, ride.PositionEnd.Longitude);
                    rideInfo.EndLocationName = await _googleGeocoding.GetAddressFromCoordinatesAsync((double)ride.PositionEnd.Latitude, (double)ride.PositionEnd.Longitude);

                    _logger.LogDebug("Address result: {Address}", rideInfo.EndLocationName);
                }
                if (ride.PositionStart != null) rideInfo.StartLocationName = await _googleGeocoding.GetAddressFromCoordinatesAsync((double)ride.PositionStart.Latitude, (double)ride.PositionStart.Longitude);

                AllRides.Add(rideInfo);
            }

            return Page();
        }
    }
}