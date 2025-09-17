using System;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Mobishare.Core.Models.UserRelated;
using Mobishare.Core.Requests.Users.TechnicianRequest.Queries;
using Swashbuckle.AspNetCore.Annotations;

namespace Mobishare.Core.Controllers.Users;

[ApiController]
[Route("api/[controller]")]

public class TechnicianApisController : ControllerBase
{
    private readonly IMediator _mediator;

    public TechnicianApisController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpGet("GetTechniciansReports")]
    [SwaggerOperation(
        Summary = "Get technician reports",
        Description = "Retrieves all technician reports.")]
    public async Task<IActionResult> GetTechniciansReports()
    {
        var query = new TechnicianReportService();
        var response = await _mediator.Send(query);

        return Ok(response);
    }
}
