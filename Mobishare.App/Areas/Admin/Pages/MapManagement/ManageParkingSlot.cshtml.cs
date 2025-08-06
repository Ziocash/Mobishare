using System.ComponentModel.DataAnnotations;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mobishare.Core.Models.Maps;
using Mobishare.Core.ParkingSlotClassification;
using Mobishare.Core.Requests.Maps.ParkingSlotRequests.Commands;
using Mobishare.Core.Security;
using Mobishare.Core.ValidationAttributes;
using NetTopologySuite.IO;

namespace Mobishare.App.Areas.Admin.Pages.MapManagement
{
    [Authorize(Policy = PolicyNames.IsStaff)]
    public class ManageParkingSlotModel : PageModel
    {
        private readonly ILogger<ManageParkingSlotModel> _logger;
        private readonly HttpClient _httpClient;
        private readonly UserManager<IdentityUser> _userManager;
        public IEnumerable<ParkingSlot> AllParkingSlots { get; set; }
        public IEnumerable<City> AllCities { get; set; }
        public string AllParkingSlotsPerimeter { get; set; }
        public string AllCitiesPerimeter { get; set; }

        [BindProperty]
        public InputParkingSlot Input { get; set; }
        public class InputParkingSlot
        {
            public int? Id { get; set; }

            [Required(ErrorMessage = "Parking slot area is required.")]
            [AvoidParkingSlotCollision(ErrorMessage = "City area intersects with an existing city.")]
            [HasParkingInCity(ErrorMessage = "Parking slot area is not in the city.")]
            public string ParkingArea { get; set; }
            [Required(ErrorMessage = "Select a valid parking slot type.")]
            public ParkingSlotTypes Type { get; set; }
        }

        public ManageParkingSlotModel(ILogger<ManageParkingSlotModel> logger,
            IHttpClientFactory httpClientFactory,
            UserManager<IdentityUser> userManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClientFactory.CreateClient("CityApi");
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public async Task<IActionResult> OnPostAddNewParkingSlot()
        {
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                await LoadAllDataAsync();
                _logger.LogWarning("User ID is null. Unable to process the request.");
                return Page();
            }

            if (!ModelState.IsValid)
            {
                await LoadAllDataAsync();
                return Page();
            }

            var citiesToCheck = await _httpClient.GetFromJsonAsync<IEnumerable<City>>("api/City/AllCities");
            var reader = new WKTReader();
            var newParkingSlotGeometry = reader.Read(Input.ParkingArea);
            bool contained = false;
            var cityId = 0;

            foreach (var cityToCheck in citiesToCheck ?? new List<City>())
            {
                var cityGeometryToCheck = reader.Read(cityToCheck.PerimeterLocation);
                contained = cityGeometryToCheck.Contains(newParkingSlotGeometry);
                if (contained)
                {
                    cityId = cityToCheck.Id;
                    break;
                }
            }

            if (!contained)
            {
                ModelState.AddModelError("Input.ParkingArea", "Parking slot area is not in the city.");
                await LoadAllDataAsync();
                return Page();
            }

            var createResponse = await _httpClient.PostAsJsonAsync("api/ParkingSlot",
                new CreateParkingSlot
                {
                    PerimeterLocation = Input.ParkingArea,
                    CreatedAt = DateTime.UtcNow,
                    CityId = cityId,
                    UserId = userId,
                    Type = Input.Type.ToString()
                }
            );

            if (!createResponse.IsSuccessStatusCode)
            {
                var errorContent = await createResponse.Content.ReadAsStringAsync();
                _logger.LogError($"API error: {createResponse.StatusCode}, Content: {errorContent}");
                TempData["ErrorMessage"] = $"Failed to add parking slot. Error: {errorContent}";
                await LoadAllDataAsync();
                return Page();
            }

            TempData["SuccessMessage"] = "Parking slot successfully added";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateParkingSlot(int id)
        {
            Input.Id = id;

            if (!ModelState.IsValid)
            {
                await LoadAllDataAsync();
                return Page();
            }

            var citiesToCheck = await _httpClient.GetFromJsonAsync<IEnumerable<City>>("api/City/AllCities");
            var reader = new WKTReader();
            var newParkingSlotGeometry = reader.Read(Input.ParkingArea);
            bool contained = false;
            var cityId = 0;

            foreach (var cityToCheck in citiesToCheck ?? new List<City>())
            {
                var cityGeometryToCheck = reader.Read(cityToCheck.PerimeterLocation);
                contained = cityGeometryToCheck.Contains(newParkingSlotGeometry);
                if (contained)
                {
                    cityId = cityToCheck.Id;
                    break;
                }
            }

            if (!contained)
            {
                ModelState.AddModelError("Input.ParkingArea", "Parking slot area is not in the city.");
                await LoadAllDataAsync();
                return Page();
            }


            var updateResponse = await _httpClient.PutAsJsonAsync("api/ParkingSlot",
                new UpdateParkingSlot
                {
                    Id = id,
                    UserId = _userManager.GetUserId(User),
                    CityId = cityId,
                    PerimeterLocation = Input.ParkingArea,
                    CreatedAt = DateTime.UtcNow,
                    Type = Input.Type.ToString()
                }
            );

            if (!updateResponse.IsSuccessStatusCode)
            {
                var errorContent = await updateResponse.Content.ReadAsStringAsync();
                _logger.LogError($"API error: {updateResponse.StatusCode}, Content: {errorContent}");
                TempData["ErrorMessage"] = $"Failed to update parking slot. Error: {errorContent}";
                await LoadAllDataAsync();
                return Page();
            }

            TempData["SuccessMessage"] = "Parking slot successfully updated";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteParkingSlot(int id)
        {
            var deleteResponse = await _httpClient.DeleteAsync($"api/ParkingSlot/{id}");

            if (!deleteResponse.IsSuccessStatusCode)
            {
                var errorContent = await deleteResponse.Content.ReadAsStringAsync();
                _logger.LogError($"API error: {deleteResponse.StatusCode}, Content: {errorContent}");
                TempData["ErrorMessage"] = $"Failed to delete parking slot. Error: {errorContent}";
            }
            else
            {
                TempData["SuccessMessage"] = "Parking slot successfully deleted";
            }

            await LoadAllDataAsync();
            return Page();
        }

        public async Task OnGet()
        {
            await LoadAllDataAsync();
        }

        private async Task LoadAllDataAsync()
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