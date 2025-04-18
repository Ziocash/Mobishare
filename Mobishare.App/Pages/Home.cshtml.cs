using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Mobishare.App.Pages
{
    public class HomeModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public string GoogleMapsApiKey { get; private set; }

        public HomeModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void OnGet()
        {
            GoogleMapsApiKey = _configuration["GoogleMaps:ApiKey"]
                ?? throw new InvalidOperationException("Google Maps API key is not configured.");
        }
    }
}
