using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mobishare.Core.Models.Vehicles;
using Mobishare.Core.Requests.Vehicles.VehicleTypeRequests.Commands;
using Mobishare.Core.Security;
using Mobishare.Core.ValidationAttributes;
using Mobishare.Core.VehicleClassification;

namespace Mobishare.App.Areas.Admin.Pages.VehicleManagement
{
    [Authorize(Policy = PolicyNames.IsStaff)]
    public class ManageVehicleTypeModel : PageModel
    {
        private readonly ILogger<ManageVehicleTypeModel> _logger;
        private readonly HttpClient _httpClient;
        public IEnumerable<VehicleType> AllVehicleTypes { get; set; }

        public ManageVehicleTypeModel(
            ILogger<ManageVehicleTypeModel> logger,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClientFactory.CreateClient("CityApi");
        }

        [BindProperty]
        public InputVehicleTypeModel Input { get; set; }

        /// <summary>
        /// Model for the input form to add a new vehicle type.
        /// Model name is required and must be unique.
        /// Vehicle type is required and must be one of the defined vehicle types.
        /// Price per minute is required and must be greater than 0.
        /// </summary>
        public class InputVehicleTypeModel
        {
            public int Id { get; set; }

            [Required(ErrorMessage = "Provide a vehicle model.")]
            [MaxLength(50, ErrorMessage = "Model name cannot be longer than 50 characters.")]
            [UniqueVehicleTypeModel(ErrorMessage = "Vehicle type with this model already exists.")]
            public string Model { get; set; }
            [Required(ErrorMessage = "Select a valid vehicle type.")]
            public VehicleTypes? Type { get; set; }
            [Required(ErrorMessage = "Provide a value.")]
            [Range(0.01, double.MaxValue, ErrorMessage = "Price per minute must be greater than 0")]
            public decimal? PricePerMinute { get; set; }
        }

        public async Task<IActionResult> OnPostAddVehicleType()
        {
            if (!ModelState.IsValid)
            {
                await LoadAllVehicleTypes();
                _logger.LogWarning("Invalid model states. Model states status: " + !ModelState.IsValid);
                return Page();
            }

            var createResponse = await _httpClient.PostAsJsonAsync("api/VehicleType",
                new CreateVehicleType
                {
                    Model = Input.Model.ToUpper(),
                    Type = Input.Type.ToString()!,
                    PricePerMinute = Input.PricePerMinute ?? 0.0m,
                    CreatedAt = DateTime.UtcNow
                }
            );

            if (!createResponse.IsSuccessStatusCode)
            {
                var errorContent = await createResponse.Content.ReadAsStringAsync();
                _logger.LogError($"API error: {createResponse.StatusCode}, Content: {errorContent}");
                TempData["ErrorMessage"] = $"Failed to add vheicle type. Error: {errorContent}";
                await LoadAllVehicleTypes();
                return Page();
            }

            _logger.LogInformation("Vehicle Type succesfully added.");
            TempData["SuccessMessage"] = "Vehicle Type succesfully added.";

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateVehicleType(int id)
        {
            Input.Id = id;
            if (!ModelState.IsValid)
            {
                await LoadAllVehicleTypes();
                _logger.LogWarning("Invalid model states. Model states status: " + !ModelState.IsValid);
                return Page();
            }

            var rawPriceValue = ModelState["Input.PricePerMinute"]?.AttemptedValue;

            var updateResponse = await _httpClient.PutAsJsonAsync("api/VehicleType",
                new UpdateVehicleType
                {
                    Id = id,
                    Model = Input.Model.ToUpper(),
                    Type = Input.Type.ToString()!,
                    PricePerMinute = decimal.Parse(rawPriceValue, CultureInfo.InvariantCulture.NumberFormat),
                    CreatedAt = DateTime.UtcNow
                }
            );

            if (!updateResponse.IsSuccessStatusCode)
            {
                var errorContent = await updateResponse.Content.ReadAsStringAsync();
                _logger.LogError($"API error: {updateResponse.StatusCode}, Content: {errorContent}");
                TempData["ErrorMessage"] = $"Failed to update vehicle type. Error: {errorContent}";
                await LoadAllVehicleTypes();
                return Page();
            }

            _logger.LogInformation("Vehicle Type succesfully updated");
            TempData["SuccessMessage"] = "Vehicle Type succesfully updated.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteVehicleType(int id)
        {
            var deleteResponse = await _httpClient.DeleteAsync($"api/VehicleType/{id}");

            if (!deleteResponse.IsSuccessStatusCode)
            {
                var errorContent = await deleteResponse.Content.ReadAsStringAsync();
                _logger.LogError($"API error: {deleteResponse.StatusCode}, Content: {errorContent}");
                TempData["ErrorMessage"] = $"Failed to delete vehicle type. Error: {errorContent}";
            }
            else
            {
                _logger.LogInformation("Vehicle Type succesflully deleted.");
                TempData["SuccessMessage"] = "Vehicle Type successfully deleted";
            }

            await LoadAllVehicleTypes();
            return Page();
        }

        public async Task OnGet()
        {
            await LoadAllVehicleTypes();
        }

        private async Task LoadAllVehicleTypes()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<IEnumerable<VehicleType>>("api/VehicleType/AllVehicleTypes");
                AllVehicleTypes = response ?? [];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading data");
            }
        }
    }
}
