using System;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Mobishare.Core.Data;
using Mobishare.Core.Models.Vehicles;

namespace Mobishare.Core.Requests.Vehicles.ReportAssignmentsRequests.Commands;

public class CreateReportAssignment : ReportAssignment, IRequest<ReportAssignment> { }

public class CreateReportAssignmentHandler : IRequestHandler<CreateReportAssignment, ReportAssignment>
{
    private readonly IMapper _mapper;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<CreateReportAssignmentHandler> _logger;

    public CreateReportAssignmentHandler(
        IMapper mapper,
        ApplicationDbContext dbContext,
        ILogger<CreateReportAssignmentHandler> logger)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ReportAssignment> Handle(CreateReportAssignment request, CancellationToken cancellationToken)
    {
        var newReportAssignments = _mapper.Map<ReportAssignment>(request);
        
        try
        {
            _logger.LogDebug("Executing {method}", nameof(CreateReportAssignment));
            _dbContext.ReportAssignments.Entry(newReportAssignments).State = Microsoft.EntityFrameworkCore.EntityState.Added;
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Report assignment {ReportAssignmentId} created successfully", newReportAssignments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating new report assignment");

        }
        return newReportAssignments;
    }
}