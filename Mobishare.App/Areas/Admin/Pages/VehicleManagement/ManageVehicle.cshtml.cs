using System.ComponentModel.DataAnnotations;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mobishare.Core.Models.Vehicles;

namespace Mobishare.App.Areas.Admin.Pages.VehicleManagement
{
    public class ManageVehicleModel : PageModel
    {
        private readonly ILogger<ManageVehicleModel> _logger;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
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
            // public int Id { get; set; }
        }
        
        public void OnGet()
        {
        }
    }
}
