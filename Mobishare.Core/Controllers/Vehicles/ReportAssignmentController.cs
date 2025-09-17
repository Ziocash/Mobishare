using System;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Mobishare.Core.Requests.Vehicles.ReportAssignmentsRequests.Commands;
using Swashbuckle.AspNetCore.Annotations;

namespace Mobishare.Core.Controllers.Vehicles;

[ApiController]
[Route("api/[controller]")]
public class ReportAssignmentController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReportAssignmentController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }
    
    [HttpPost()]
    [SwaggerOperation(
        Summary = "Create a new report assignment",
        Description = "This endpoint allows you to create a new report assignment.",
        OperationId = "CreateReportAssignment")]
    [SwaggerResponse(201, "Report assignment created successfully", typeof(CreateReportAssignment))]
    [SwaggerResponse(400, "Invalid request payload")]
    public async Task<IActionResult> CreateReportAssignment(
        [FromBody] 
        [SwaggerParameter(Description = "The report assignment details to create.", Required = true)]
        CreateReportAssignment request
    )
    {
        if (request == null)
        {
            return BadRequest("Request payload cannot be null.");
        }

        var result = await _mediator.Send(request);
        if (result == null)
        {
            return BadRequest("Failed to create report assignment.");
        }

        return CreatedAtAction(nameof(CreateReportAssignment), new { id = result.Id }, result);
    }
}
