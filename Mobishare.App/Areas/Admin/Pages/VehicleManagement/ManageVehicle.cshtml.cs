using System.ComponentModel.DataAnnotations;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mobishare.Core.Models.Maps;
using Mobishare.Core.Models.Vehicles;
using Mobishare.Core.Requests.Maps.CityRequests.Queries;
using Mobishare.Core.Requests.Vehicles.VehicleRequests.Queries;
using Mobishare.Core.Requests.Vehicles.VehicleTypeRequests.Queries;

namespace Mobishare.App.Areas.Admin.Pages.VehicleManagement
{
    public class ManageVehicleModel : PageModel
    {
        private readonly ILogger<ManageVehicleModel> _logger;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        public IEnumerable<VehicleType> AllVehicleTypes { get; set; }
        public IEnumerable<City> AllCities { get; set; }
        public IEnumerable<Vehicle> AllVehicles { get; set; }
        
        public ManageVehicleModel(ILogger<ManageVehicleModel> logger, IMediator mediator, IMapper mapper)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
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
            public int? VehicleTypeId { get; set; }
            [Required(ErrorMessage = "Select a valid city.")]
            public int? CityId { get; set; }
        }
        
        public async Task<IActionResult> OnPostAddVehicle()
        {
            if (!ModelState.IsValid)
            {
                AllVehicleTypes = await _mediator.Send(new GetAllVehicleType());
                AllCities = await _mediator.Send(new GetAllCities());
                AllVehicles = await _mediator.Send(new GetAllVehicles());

                _logger.LogWarning("Invalid model states. Model states status: " + !ModelState.IsValid);
                return Page();
            }

            var vehicle = _mapper.Map<Vehicle>(Input);
            /*var result = await _mediator.Send(new AddVehicleCommand(vehicle));

            if (result)
            {
                return RedirectToPage("/Index");
            }*/

            ModelState.AddModelError(string.Empty, "Failed to add vehicle.");
            return RedirectToPage();
        }

        public async Task OnGet()
        {
            AllVehicleTypes = await _mediator.Send(new GetAllVehicleType());
            AllCities = await _mediator.Send(new GetAllCities());
            AllVehicles = await _mediator.Send(new GetAllVehicles());
        }
    }
}
