using System;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Mobishare.Core.Models.Vehicles;
using Mobishare.Core.Requests.Vehicles.ReportRequests;
using Mobishare.Core.Requests.Vehicles.ReportRequests.Commands;
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

    [HttpPost()]
    [SwaggerOperation(
        Summary = "Create a new report",
        Description = "This endpoint allows you to create a new report.",
        OperationId = "CreateReport")]
    [SwaggerResponse(201, "Report created successfully", typeof(CreateReport))]
    [SwaggerResponse(400, "Invalid request payload")]
    public async Task<IActionResult> CreateReport(
        [FromBody] 
        [SwaggerParameter(Description = "The report details to create.", Required = true)]
        CreateReport request
    )
    {
        if (request == null)
        {
            return BadRequest("Request payload cannot be null.");
        }

        var result = await _mediator.Send(request);
        if (result == null)
        {
            return BadRequest("Failed to create report.");
        }

        return CreatedAtAction(nameof(CreateReport), new { id = result.Id }, result);
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

    [HttpPut("Assign")]
    [SwaggerOperation(
        Summary = "Change report status",
        Description = "This endpoint change report status to Assigned.",
        OperationId = "UpdateReport"
    )]
    [SwaggerResponse(200, "Reports retrieved successfully", typeof(UpdateReport))]
    [SwaggerResponse(400, "Invalid request payload")]
    [SwaggerResponse(404, "User not found or no reports found")]
    public async Task<IActionResult> AssignReport(
        [FromBody]
        [SwaggerParameter("reportId", Required = true, Description = "The ID of the report to update.")]
        UpdateReport updateReport)
    {
        if (updateReport == null)
        {
            return BadRequest("Invalid request payload.");
        }

        var result = await _mediator.Send(updateReport);

        if (result == null)
        {
            return NotFound("Report not found.");
        }

        return Ok(result);
    }
    
    [HttpPut("Close")]
    [SwaggerOperation(
        Summary = "Change report status",
        Description = "This endpoint change report status to Closed.",
        OperationId = "UpdateReport"
    )]
    [SwaggerResponse(200, "Reports retrieved successfully", typeof(UpdateReport))]
    [SwaggerResponse(400, "Invalid request payload")]
    [SwaggerResponse(404, "User not found or no reports found")]
    public async Task<IActionResult> CloseReport(
        [FromBody]
        [SwaggerParameter("reportId", Required = true, Description = "The ID of the report to update.")]
        UpdateReport updateReport)
    {
        if (updateReport == null)
        {
            return BadRequest("Invalid request payload.");
        }

        var result = await _mediator.Send(updateReport);
        
        if (result == null)
        {
            return NotFound("Report not found.");
        }

        return Ok(result);
    }
}
