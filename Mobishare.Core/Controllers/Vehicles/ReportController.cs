using System;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Mobishare.Core.Models.Vehicles;
using Mobishare.Core.Requests.Vehicles.ReportRequests;
using Swashbuckle.AspNetCore.Annotations;

namespace Mobishare.Core.Controllers.Vehicles;

[ApiController]
[Route("api/[controller]")]
public class ReportController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReportController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpGet("AllReports/{userId}")]
    [SwaggerOperation(
        Summary = "Get all reports for a technician",
        Description = "This endpoint retrieves all reports for a specific user.",
        OperationId = "GetReportsByUserId"
    )]
    [SwaggerResponse(200, "Reports retrieved successfully", typeof(IEnumerable<Report>))]
    [SwaggerResponse(404, "User not found or no reports found")]
    public async Task<IActionResult> GetReportsByUserId(
        [FromRoute]
        [SwaggerParameter("userId", Required = true, Description = "The ID of the user whose reports are to be retrieved")]
        string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest("User ID cannot be null or empty.");
        }

        var result = await _mediator.Send(new GetReportsByUserId(userId));
        
        return Ok(result);
    }
}
