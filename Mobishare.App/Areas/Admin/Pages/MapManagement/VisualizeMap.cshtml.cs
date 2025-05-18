using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mobishare.Core.Models.Maps;
using Mobishare.Core.Requests.Maps.CityRequests.Queries;
using Mobishare.Core.Requests.Maps.ParkingSlotRequests.Queries;

namespace Mobishare.App.Areas.Admin.Pages.MapManagement
{
    public class VisualizeMapModel : PageModel
    {
        private readonly ILogger<VisualizeMapModel> _logger;
        private readonly IMediator _mediator;
        private readonly IConfiguration _configuration;
        public IEnumerable<ParkingSlot> AllParkingSlots { get; set; }
        public IEnumerable<City> AllCities { get; set; }
        public string AllParkingSlotsPerimeter { get; set; }
        public string AllCitiesPerimeter { get; set; }
        public string GoogleMapsApiKey { get; private set; }

        public VisualizeMapModel(ILogger<VisualizeMapModel> logger, IMediator mediator, IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task OnGet()
        {
            GoogleMapsApiKey = _configuration["GoogleMaps:ApiKey"]
            ?? throw new InvalidOperationException("Google Maps API key is not configured.");
            AllCities = await _mediator.Send(new GetAllCities());
            foreach (var city in AllCities) AllCitiesPerimeter += city.PerimeterLocation + ";";

            AllParkingSlots = await _mediator.Send(new GetAllParkingSlots());
            foreach (var parkingSlot in AllParkingSlots) AllParkingSlotsPerimeter += parkingSlot.PerimeterLocation + ";";
        }
    }
}
