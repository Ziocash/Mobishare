using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mobishare.Core.Models.Vehicles;
using Mobishare.Core.ReportStatusEnum;
using Mobishare.Core.Requests.Vehicles.ReportRequests.Commands;
using Mobishare.Core.Security;

namespace Mobishare.App.Areas.Admin.Pages.Reports
{
    [Authorize(Policy = PolicyNames.IsTechnician)]
    public class ManageTicketsModel : PageModel
    {
        private readonly ILogger<ManageTicketsModel> _logger;
        private readonly HttpClient _httpClient;
        private readonly UserManager<IdentityUser> _userManager;
        public IEnumerable<Report> AllReports { get; set; }

        public ManageTicketsModel(
            ILogger<ManageTicketsModel> logger,
            IHttpClientFactory httpClientFactory,
            UserManager<IdentityUser> userManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClientFactory.CreateClient("CityApi");
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            public int ReportId { get; set; }

            [Required(ErrorMessage = "Please provide a description of the work done.")]
            [StringLength(500, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 500 characters.")]
            public string RepairDescription { get; set; }

            // public string NewStatus { get; set; }
        }

        public async Task<IActionResult> OnPostAssignTicket(int id)
        {
            var userId = _userManager.GetUserId(User);
            _logger.LogInformation($"User {userId} is assigning ticket {id}.");

            var response = await _httpClient.PutAsJsonAsync($"api/Report/Assign",
                new UpdateReport
                {
                    Id = id,
                    Status = ReportStatus.Assigned.ToString()    
                }
            );

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"API error while assigning ticket: {response.StatusCode}, Content: {errorContent}");
                TempData["ErrorMessage"] = "Failed to assign the ticket.";
            }
            else
            {
                _logger.LogInformation($"Ticket {id} assigned successfully to user {userId}.");
                TempData["SuccessMessage"] = "Ticket successfully assigned to you.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCloseTicket()
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("CloseTicket model state is invalid.");
                await LoadAllReports();
                return Page();
            }

            _logger.LogInformation($"Closing ticket {Input.ReportId}.");

            var response = await _httpClient.PutAsJsonAsync($"api/Report/Close",
                new UpdateReport
                {
                    Id = Input.ReportId,
                    Status = ReportStatus.Closed.ToString()
                }
            );

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"API error while closing ticket: {response.StatusCode}, Content: {errorContent}");
                TempData["ErrorMessage"] = "Failed to update the ticket.";
            }
            else
            {
                _logger.LogInformation($"Ticket {Input.ReportId} was successfully closed.");
                TempData["SuccessMessage"] = "Ticket has been successfully updated.";
            }

            return RedirectToPage();
        }

        public async Task OnGetAsync()
        {
            await LoadAllReports();
        }

        private async Task LoadAllReports()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                _logger.LogWarning("User ID is null.");
                AllReports = [];
                return;
            }

            try
            {
                var response = await _httpClient.GetFromJsonAsync<IEnumerable<Report>>($"api/Report/AllReports/{userId}");
                AllReports = response ?? [];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading reports from API.");
                AllReports = [];
                TempData["ErrorMessage"] = "Could not load ticket data.";
            }
        }
    }
}
