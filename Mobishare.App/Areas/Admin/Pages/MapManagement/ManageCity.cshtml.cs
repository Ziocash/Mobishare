using System.ComponentModel.DataAnnotations;
using System.Globalization;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mobishare.Core.Models.Maps;
using Mobishare.Core.Requests.Maps.CityRequests.Commands;
using Mobishare.Core.Requests.Maps.CityRequests.Queries;
using Mobishare.Core.ValidationAttributes;

namespace Mobishare.App.Areas.Admin.Pages.MapManagement
{
    public class ManageCityModel : PageModel
    {
        private readonly ILogger<ManageCityModel> _logger;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly UserManager<IdentityUser> _userManager;
        public IEnumerable<City> AllCities { get; set; }
        public string AllCitiesPerimeter { get; set; }

        /// <param name="configuration"></param>  
        /// <remarks>
        /// This constructor initializes the ManageCityModel with the provided configuration.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when configuration is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when Google Maps API key is not configured.</exception>
        public ManageCityModel(ILogger<ManageCityModel> logger, IMediator mediator, IMapper mapper, UserManager<IdentityUser> userManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        [BindProperty]
        public InputCityModel Input { get; set; }
        public class InputCityModel
        {
            public int Id { get; set; }

            [Required(ErrorMessage = "City area is required.")]
            [AvoidCitiesCollision(ErrorMessage = "City area intersects with an existing city.")]
            public string CityArea { get; set; }
            [Required(ErrorMessage = "City name is required.")]
            [UniqueCityName(ErrorMessage = "City already exists.")]
            public string CityName { get; set; }
        }

        /// <summary>
        /// Handles the form submission for adding a new city.
        /// </summary>
        public async Task<IActionResult> OnPostAddNewCity()
        {
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                AllCities = await _mediator.Send(new GetAllCities());

                foreach (var city in AllCities) AllCitiesPerimeter += city.PerimeterLocation + ";";

                _logger.LogWarning("User ID is null.");
                return Page();
            }

            if (!ModelState.IsValid)
            {
                AllCities = await _mediator.Send(new GetAllCities());

                foreach (var city in AllCities) AllCitiesPerimeter += city.PerimeterLocation + ";";

                _logger.LogWarning("Invalid model states. Model states status: " + !ModelState.IsValid);
                return Page();
            }

            await _mediator.Send(_mapper.Map<CreateCity>(
                new City
                {
                    Name = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(Input.CityName),
                    PerimeterLocation = Input.CityArea,
                    CreatedAt = DateTime.UtcNow,
                    UserId = userId
                }));

            _logger.LogInformation("City successfully added.");
            TempData["SuccessMessage"] = "City successfully added.";

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateCity(int id)
        {
            Input.Id = id;
            if (!ModelState.IsValid)
            {
                AllCities = await _mediator.Send(new GetAllCities());
                foreach (var city in AllCities) AllCitiesPerimeter += city.PerimeterLocation + ";";

                _logger.LogWarning("Invalid model states. Model states status: " + !ModelState.IsValid);

                return Page();
            }

            await _mediator.Send(new UpdateCity
            {
                Id = id,
                UserId = _userManager.GetUserId(User),
                Name = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(Input.CityName),
                PerimeterLocation = Input.CityArea,
                CreatedAt = DateTime.UtcNow
            });

            _logger.LogInformation("City successfully updated.");
            TempData["SuccessMessage"] = "City successfully updated.";

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteCity(int id)
        {
            await _mediator.Send(_mapper.Map<DeleteCity>(
                new City
                {
                    Id = id
                }));

            _logger.LogInformation("City succesflully deleted.");
            TempData["SuccessMessage"] = "City succesflully deleted.";

            AllCities = await _mediator.Send(new GetAllCities());
            foreach (var city in AllCities) AllCitiesPerimeter += city.PerimeterLocation + ";";

            return Page();
        }

        public async Task OnGet()
        {
            AllCities = await _mediator.Send(new GetAllCities());
            foreach (var city in AllCities) AllCitiesPerimeter += city.PerimeterLocation + ";";
        }
    }
}
