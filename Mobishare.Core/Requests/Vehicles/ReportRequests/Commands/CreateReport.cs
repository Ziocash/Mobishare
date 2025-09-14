using System;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Mobishare.Core.Data;
using Mobishare.Core.Models.Vehicles;

namespace Mobishare.Core.Requests.Vehicles.ReportRequests.Commands;

public class CreateReport : Report, IRequest<Report> { }

public class CreateReportHandler : IRequestHandler<CreateReport, Report>
{
    private readonly IMapper _mapper;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<CreateReportHandler> _logger;

    public CreateReportHandler(
        IMapper mapper,
        ApplicationDbContext dbContext,
        ILogger<CreateReportHandler> logger)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Report> Handle(CreateReport request, CancellationToken cancellationToken)
    {
        var newReport = _mapper.Map<Report>(request);
        
        try
        {
            _logger.LogDebug("Executing {method}", nameof(CreateReport));
            _dbContext.Reports.Entry(newReport).State = Microsoft.EntityFrameworkCore.EntityState.Added;
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Report {ReportId} created successfully", newReport);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating new report");

        }
        
        return newReport;
    }
}