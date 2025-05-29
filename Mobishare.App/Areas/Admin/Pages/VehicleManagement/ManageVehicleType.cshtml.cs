using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mobishare.Core.Models.Vehicles;
using Mobishare.Core.Requests.Vehicles.VehicleTypeRequests.Commands;
using Mobishare.Core.Requests.Vehicles.VehicleTypeRequests.Queries;
using Mobishare.Core.Security;
using Mobishare.Core.ValidationAttributes;
using Mobishare.Core.VehicleClassification;

namespace Mobishare.App.Areas.Admin.Pages.VehicleManagement
{
    [Authorize(Policy = PolicyNames.IsStaff)]
    public class ManageVehicleTypeModel : PageModel
    {
        private readonly ILogger<ManageVehicleTypeModel> _logger;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        public IEnumerable<VehicleType> AllVehicleTypes { get; set; }

        public ManageVehicleTypeModel(ILogger<ManageVehicleTypeModel> logger, IMediator mediator, IMapper mapper)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
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
                AllVehicleTypes = await _mediator.Send(new GetAllVehicleType());

                _logger.LogWarning("Invalid model states. Model states status: " + !ModelState.IsValid);

                return Page();
            }



            await _mediator.Send(_mapper.Map<CreateVehicleType>(
                new VehicleType
                {
                    Model = Input.Model.ToUpper(),
                    Type = Input.Type.ToString()!,
                    PricePerMinute = Input.PricePerMinute ?? 0.0m,
                    CreatedAt = DateTime.UtcNow
                }));

            _logger.LogInformation("Vehicle Type succesfully added.");
            TempData["SuccessMessage"] = "Vehicle Type succesfully added.";

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateVehicleType(int id)
        {
            Input.Id = id;
            if (!ModelState.IsValid)
            {
                AllVehicleTypes = await _mediator.Send(new GetAllVehicleType());

                _logger.LogWarning("Invalid model states. Model states status: " + !ModelState.IsValid);

                return Page();
            }

            var rawPriceValue = ModelState["Input.PricePerMinute"]?.AttemptedValue;

            await _mediator.Send(_mapper.Map<UpdateVehicleType>(
                new VehicleType
                {
                    Id = id,
                    Model = Input.Model.ToUpper(),
                    Type = Input.Type.ToString()!,
                    PricePerMinute = decimal.Parse(rawPriceValue, CultureInfo.InvariantCulture.NumberFormat),
                    CreatedAt = DateTime.UtcNow
                }
            ));

            _logger.LogInformation("Vehicle Type succesfully updated");
            TempData["SuccessMessage"] = "Vehicle Type succesfully updated.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteVehicleType(int id)
        {
            await _mediator.Send(_mapper.Map<DeleteVehicleType>(
                new VehicleType
                {
                    Id = id
                }));

            _logger.LogInformation("Vehicle Type succesflully deleted.");
            TempData["SuccessMessage"] = "Vehicle Type succesflully deleted.";

            AllVehicleTypes = await _mediator.Send(new GetAllVehicleType());

            return Page();
        }

        public async Task OnGet()
        {
            AllVehicleTypes = await _mediator.Send(new GetAllVehicleType());
        }
    }
}
