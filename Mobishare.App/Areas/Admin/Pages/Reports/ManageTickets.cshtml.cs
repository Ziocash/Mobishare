using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mobishare.Core.Models.Vehicles;
using Mobishare.Core.Security;

namespace MyApp.Namespace
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

        public async Task OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                _logger.LogWarning("User ID is null.");
            } else {
                var response = await _httpClient.GetFromJsonAsync<IEnumerable<Report>>($"api/Report/AllReports/{userId}");
                AllReports = response ?? [];
            }
        }
    }
}
