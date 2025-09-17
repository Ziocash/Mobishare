using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mobishare.Core.Models.Maps;
using Mobishare.Core.Models.Vehicles;
using Mobishare.Core.Requests.Vehicles.VehicleRequests.Commands;
using Mobishare.Core.Security;
using Mobishare.Core.VehicleStatus;

namespace Mobishare.App.Areas.Admin.Pages.VehicleManagement
{
    [Authorize(Policy = PolicyNames.IsStaff)]
    public class ManageVehicleModel : PageModel
    {
        private readonly ILogger<ManageVehicleModel> _logger;
        private readonly HttpClient _httpClient;
        public IEnumerable<VehicleType> AllVehicleTypes { get; set; }
        public IEnumerable<ParkingSlot> AllParkingSlot { get; set; }
        public IEnumerable<Vehicle> AllVehicles { get; set; }

        public ManageVehicleModel(
            ILogger<ManageVehicleModel> logger,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClientFactory.CreateClient("CityApi");
        }

        [BindProperty]
        public InputVehicleModel Input { get; set; }

        /// <summary>
        /// Model for the input form to add a new vehicle.
        /// <summary>
        public class InputVehicleModel
        {
            public int Id { get; set; }
            [MaxLength(5, ErrorMessage = "Plate must be long exactly 5 characters.")]
            [MinLength(5, ErrorMessage = "Invalid, plate must be long exactly 5 characters.")]
            public string? Plate { get; set; }
            [Required(ErrorMessage = "Select a valid vehicle type.")]
            public int VehicleTypeId { get; set; }
            [Required(ErrorMessage = "Select a valid city.")]
            public int ParkingSlotId { get; set; }
            [Required(ErrorMessage = "Select a valid status.")]
            public VehicleStatusType Status { get; set; }
        }

        public async Task<IActionResult> OnPostAddVehicle()
        {
            if (!ModelState.IsValid)
            {
                await LoadAllData();

                _logger.LogWarning("Invalid model states. Model states status: " + !ModelState.IsValid);
                return Page();
            }

            var createResponse = await _httpClient.PostAsJsonAsync("api/Vehicle",
                new CreateVehicle
                {
                    Plate = Input.Plate ?? string.Empty,
                    Status = VehicleStatusType.Free.ToString(),
                    VehicleTypeId = Input.VehicleTypeId,
                    ParkingSlotId = Input.ParkingSlotId,
                    CreatedAt = DateTime.UtcNow
                }
            );

            if (!createResponse.IsSuccessStatusCode)
            {
                var errorContent = await createResponse.Content.ReadAsStringAsync();
                _logger.LogError($"API error: {createResponse.StatusCode}, Content: {errorContent}");
                TempData["ErrorMessage"] = $"Failed to add vheicle. Error: {errorContent}";
                await LoadAllData();
                return Page();
            }

            _logger.LogInformation("Vehicle succesfully added.");
            TempData["SuccessMessage"] = "Vehicle succesfully added.";

            // ModelState.AddModelError(string.Empty, "Failed to add vehicle.");
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateVehicle(int id)
        {
            Input.Id = id;
            if (!ModelState.IsValid)
            {
                await LoadAllData();
                _logger.LogWarning("Invalid model states. Model states status: " + !ModelState.IsValid);
                return Page();
            }

            var updateResponse = await _httpClient.PutAsJsonAsync("api/Vehicle",
                new UpdateVehicle
                {
                    Id = id,
                    Plate = Input.Plate ?? string.Empty,
                    VehicleTypeId = Input.VehicleTypeId,
                    ParkingSlotId = Input.ParkingSlotId,
                    Status = Input.Status.ToString(),
                    CreatedAt = DateTime.UtcNow
                }
            );

            if (!updateResponse.IsSuccessStatusCode)
            {
                var errorContent = await updateResponse.Content.ReadAsStringAsync();
                _logger.LogError($"API error: {updateResponse.StatusCode}, Content: {errorContent}");
                TempData["ErrorMessage"] = $"Failed to update vehicle. Error: {errorContent}";
                await LoadAllData();
                return Page();
            }

            _logger.LogInformation("Vehicle succesfully updated");
            TempData["SuccessMessage"] = "Vehicle succesfully updated.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteVehicle(int id)
        {
            var deleteResponse = await _httpClient.DeleteAsync($"api/Vehicle/{id}");

            if (!deleteResponse.IsSuccessStatusCode)
            {
                var errorContent = await deleteResponse.Content.ReadAsStringAsync();
                _logger.LogError($"API error: {deleteResponse.StatusCode}, Content: {errorContent}");
                TempData["ErrorMessage"] = $"Failed to delete vehicle. Error: {errorContent}";
            }
            else
            {
                _logger.LogInformation("Vehicle succesflully deleted.");
                TempData["SuccessMessage"] = "Vehicle successfully deleted";
            }

            await LoadAllData();
            return Page();
        }

        public async Task OnGet()
        {
            await LoadAllData();
        }

        private async Task LoadAllData()
        {
            try
            {
                var vehicleTypeResponse = await _httpClient.GetFromJsonAsync<IEnumerable<VehicleType>>("api/VehicleType/AllVehicleTypes");
                var parkingSlotsResponse = await _httpClient.GetFromJsonAsync<IEnumerable<ParkingSlot>>("api/ParkingSlot/AllParkingSlots");
                var vehiclesResponse = await _httpClient.GetFromJsonAsync<IEnumerable<Vehicle>>("api/Vehicle/AllVehicles");

                AllVehicleTypes = vehicleTypeResponse ?? [];
                AllParkingSlot = parkingSlotsResponse ?? [];
                AllVehicles = vehiclesResponse ?? [];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading data");
            }
        }
    }
}
