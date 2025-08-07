using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mobishare.Core.Models.Maps;
using Mobishare.Core.Security;

namespace Mobishare.App.Areas.Admin.Pages.MapManagement
{
    [Authorize(Policy = PolicyNames.IsStaff)]
    public class VisualizeMapModel : PageModel
    {
        private readonly ILogger<VisualizeMapModel> _logger;
        private readonly HttpClient _httpClient;
        public IEnumerable<ParkingSlot> AllParkingSlots { get; set; }
        public IEnumerable<City> AllCities { get; set; }
        public string AllParkingSlotsPerimeter { get; set; }
        public string AllCitiesPerimeter { get; set; }

        public VisualizeMapModel(ILogger<VisualizeMapModel> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClientFactory.CreateClient("CityApi");
        }

        public async Task OnGet()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<IEnumerable<City>>("api/City/AllCities");
                AllCities = response ?? [];
                var parkigSlotResponse = await _httpClient.GetFromJsonAsync<IEnumerable<ParkingSlot>>("api/ParkingSlot/AllParkingSlots");
                AllParkingSlots = parkigSlotResponse ?? [];

                AllCitiesPerimeter = string.Join(";", AllCities.Select(c => c.PerimeterLocation));
                AllParkingSlotsPerimeter = string.Join(";", AllParkingSlots.Select(p => p.PerimeterLocation));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading data");
            }
        }
    }
}
