using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mobishare.App.Services;
using Mobishare.Core.Models.Vehicles;

namespace Mobishare.App.Pages
{

    public class TravelModel : PageModel
    {
        private readonly ILogger<LandingPageModel> _logger;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IGoogleGeocodingService _googleGeocoding;
        public String? StartLocationName;
        public Ride Ride;
        public Position? StartPosition;

        public TravelModel(
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

            StartPosition = await _httpClient.GetFromJsonAsync<Position>($"api/Position/{Ride.VehicleId}");

            if (StartPosition != null)
                StartLocationName = await _googleGeocoding.GetAddressFromCoordinatesAsync((double)StartPosition.Latitude, (double)StartPosition.Longitude);

            return Page();
        }
    }
}
