using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mobishare.App.Services;
using Mobishare.Core.Requests.Vehicles.RideRequests.Queries;
using Mobishare.Core.UiModels;

namespace Mobishare.App.Pages
{
    public class LandingPageModel : PageModel
    {
        private readonly ILogger<LandingPageModel> _logger;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IGoogleGeocodingService _googleGeocoding;

        public List<RideDisplayInfo> AllRides { get; set; } = [];

        public LandingPageModel(
            ILogger<LandingPageModel> logger,
            IMediator mediator,
            IMapper mapper,
            IConfiguration configuration,
            IGoogleGeocodingService googleGeocoding)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _googleGeocoding = googleGeocoding ?? throw new ArgumentNullException(nameof(googleGeocoding));
        }


        /*public async Task<IActionResult> OnPostReserveVehicleAsync(int vehicleId)
        {
            _logger.LogInformation("Prenotazione confermata per veicolo {VehicleId}", vehicleId);

            await _mediator.Send(new ReserveVehicleCommand(vehicleId, User.Identity?.Name ?? "Anonimo"));

            TempData["SuccessMessage"] = $"Veicolo {vehicleId} prenotato con successo!";
            return RedirectToPage();
        }*/

        public async Task<IActionResult> OnGet()
        {
            if(User?.Identity == null || !User.Identity.IsAuthenticated)
                return RedirectToPage("/Index");


            var rides = await _mediator.Send(new GetAllRides());

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