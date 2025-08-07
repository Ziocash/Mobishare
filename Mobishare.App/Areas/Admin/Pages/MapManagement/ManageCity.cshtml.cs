using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mobishare.Core.Models.Maps;
using Mobishare.Core.Requests.Maps.CityRequests.Commands;
using Mobishare.Core.Security;
using Mobishare.Core.ValidationAttributes;

namespace Mobishare.App.Areas.Admin.Pages.MapManagement
{
    [Authorize(Policy = PolicyNames.IsStaff)]
    public class ManageCityModel : PageModel
    {
        private readonly ILogger<ManageCityModel> _logger;
        private readonly HttpClient _httpClient;
        private readonly UserManager<IdentityUser> _userManager;
        public IEnumerable<City> AllCities { get; set; }
        public string AllCitiesPerimeter { get; set; }

        /// <param name="configuration"></param>  
        /// <remarks>
        /// This constructor initializes the ManageCityModel with the provided configuration.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when configuration is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when Google Maps API key is not configured.</exception>
        public ManageCityModel(
            ILogger<ManageCityModel> logger,
            IHttpClientFactory httpClientFactory,
            UserManager<IdentityUser> userManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClientFactory.CreateClient("CityApi");
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
                await LoadCitiesAsync();
                _logger.LogWarning("User ID is null.");
                return Page();
            }

            if (!ModelState.IsValid)
            {
                await LoadCitiesAsync();
                _logger.LogWarning("Invalid model states. Model states status: " + !ModelState.IsValid);
                return Page();
            }

            var createResponse = await _httpClient.PostAsJsonAsync("api/City",
                new CreateCity
                {
                    Name = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(Input.CityName),
                    PerimeterLocation = Input.CityArea,
                    CreatedAt = DateTime.UtcNow,
                    UserId = userId
                }
            );

            if (!createResponse.IsSuccessStatusCode)
            {
                var errorContent = await createResponse.Content.ReadAsStringAsync();
                _logger.LogError($"API error: {createResponse.StatusCode}, Content: {errorContent}");
                TempData["ErrorMessage"] = $"Failed to add city. Error: {errorContent}";
                await LoadCitiesAsync();
                return Page();
            }

            TempData["SuccessMessage"] = "City added successfully.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateCity(int id)
        {
            Input.Id = id;
            if (!ModelState.IsValid)
            {
                await LoadCitiesAsync();
                _logger.LogWarning("Invalid model states. Model states status: " + !ModelState.IsValid);
                return Page();
            }

            var updateResponse = await _httpClient.PutAsJsonAsync("api/City",
                new UpdateCity
                {
                    Id = id,
                    UserId = _userManager.GetUserId(User),
                    Name = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(Input.CityName),
                    PerimeterLocation = Input.CityArea,
                    CreatedAt = DateTime.UtcNow
                }
            );

            if (!updateResponse.IsSuccessStatusCode)
            {
                var errorContent = await updateResponse.Content.ReadAsStringAsync();
                _logger.LogError($"API error: {updateResponse.StatusCode}, Content: {errorContent}");
                TempData["ErrorMessage"] = $"Failed to update city. Error: {errorContent}";
                await LoadCitiesAsync();
                return Page();
            }

            _logger.LogInformation("City successfully updated.");
            TempData["SuccessMessage"] = "City successfully updated.";

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteCity(int id)
        {
            var deleteResponse = await _httpClient.DeleteAsync($"api/City/{id}");

            if (!deleteResponse.IsSuccessStatusCode)
            {
                var errorContent = await deleteResponse.Content.ReadAsStringAsync();
                _logger.LogError($"API error: {deleteResponse.StatusCode}, Content: {errorContent}");
                TempData["ErrorMessage"] = $"Failed to delete city. Error: {errorContent}";
            }
            else
            {
                _logger.LogInformation("City succesflully deleted.");
                TempData["SuccessMessage"] = "City successfully deleted";
            }

            await LoadCitiesAsync();
            return Page();
        }

        public async Task OnGet()
        {
            await LoadCitiesAsync();
        }

        private async Task LoadCitiesAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<City>>("api/City/AllCities");
            AllCities = response ?? [];
            AllCitiesPerimeter = string.Join(";", AllCities.Select(c => c.PerimeterLocation));
        }
    }
}
