using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mobishare.App.Services;
using Mobishare.Core.Models.Vehicles;
using Mobishare.Core.Requests.Vehicles.RideRequests.Queries;
using Mobishare.Core.UiModels;

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

            if (Ride.PositionStartId != null)
                StartLocationName = await _googleGeocoding.GetAddressFromCoordinatesAsync((double)Ride.PositionStart.Latitude, (double)Ride.PositionStart.Longitude);
            return Page();
        }
    }
}
