using System.ComponentModel.DataAnnotations;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mobishare.Core.Models.Maps;

namespace Mobishare.App.Areas.Admin.Pages.MapManagement
{
    public class ManageParkingSlotModel : PageModel
    {
        private readonly ILogger<ManageParkingSlotModel> _logger;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly UserManager<IdentityUser> _userManager;
        public IEnumerable<ParkingSlot> AllParkingSlots { get; set; }
        public string AllParkingSlotsPerimeter { get; set; }

        public string GoogleMapsApiKey { get; private set; }

        public ManageParkingSlotModel(ILogger<ManageParkingSlotModel> logger, IMediator mediator, IMapper mapper, IConfiguration configuration, UserManager<IdentityUser> userManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        [BindProperty]
        public InputParkingSlot Input { get; set; }
        public class InputParkingSlot
        {
            public int Id { get; set; }

            [Required(ErrorMessage = "City area is required.")]
            // [AvoidCitiesCollision(ErrorMessage = "City area intersects with an existing city.")]
            public string ParkingArea { get; set; }
    
        }

        public async Task OnGet()
        {
            GoogleMapsApiKey = _configuration["GoogleMaps:ApiKey"]
            ?? throw new InvalidOperationException("Google Maps API key is not configured.");
        }
    }


}
