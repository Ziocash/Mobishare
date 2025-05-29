using System.ComponentModel.DataAnnotations;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mobishare.Core.Models.Maps;
using Mobishare.Core.Models.Vehicles;
using Mobishare.Core.Requests.Maps.CityRequests.Queries;
using Mobishare.Core.Requests.Maps.ParkingSlotRequests.Queries;
using Mobishare.Core.Requests.Vehicles.VehicleRequests.Commands;
using Mobishare.Core.Requests.Vehicles.VehicleRequests.Queries;
using Mobishare.Core.Requests.Vehicles.VehicleTypeRequests.Queries;
using Mobishare.Core.Security;
using Mobishare.Core.VehicleStatus;

namespace Mobishare.App.Areas.Admin.Pages.VehicleManagement
{
    [Authorize(Policy = PolicyNames.IsStaff)]
    public class ManageVehicleModel : PageModel
    {
        private readonly ILogger<ManageVehicleModel> _logger;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        public IEnumerable<VehicleType> AllVehicleTypes { get; set; }
        public IEnumerable<ParkingSlot> AllParkingSlot { get; set; }
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
            public int VehicleTypeId { get; set; }
            [Required(ErrorMessage = "Select a valid city.")]
            public int ParkingSlotId { get; set; }
        }

        public async Task<IActionResult> OnPostAddVehicle()
        {
            if (!ModelState.IsValid)
            {
                AllVehicleTypes = await _mediator.Send(new GetAllVehicleType());
                AllParkingSlot = await _mediator.Send(new GetAllParkingSlots());
                AllVehicles = await _mediator.Send(new GetAllVehicles());

                _logger.LogWarning("Invalid model states. Model states status: " + !ModelState.IsValid);
                return Page();
            }

            await _mediator.Send(_mapper.Map<CreateVehicle>(
                new Vehicle
                {
                    Plate = Input.Plate ?? string.Empty,
                    Status = VehicleStatusType.Free.ToString(),
                    VehicleTypeId = Input.VehicleTypeId,
                    ParkingSlotId = Input.ParkingSlotId,
                    CreatedAt = DateTime.UtcNow
                }
            ));

            ModelState.AddModelError(string.Empty, "Failed to add vehicle.");
            return RedirectToPage();
        }

        public async Task OnGet()
        {
            AllVehicleTypes = await _mediator.Send(new GetAllVehicleType());
            AllParkingSlot = await _mediator.Send(new GetAllParkingSlots());
            AllVehicles = await _mediator.Send(new GetAllVehicles());
        }
    }
}
