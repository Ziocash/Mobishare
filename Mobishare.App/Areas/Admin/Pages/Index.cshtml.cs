using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Mobishare.App.Areas.Admin.Pages
{
    // [Authorize(Policy = PolicyNames.IsStaff)]
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public string GoogleMapsApiKey { get; private set; }
        public IndexModel(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }
        public void OnGet()
        {
            GoogleMapsApiKey = _configuration["GoogleMaps:ApiKey"]
            ?? throw new InvalidOperationException("Google Maps API key is not configured.");
        }
    }
}
