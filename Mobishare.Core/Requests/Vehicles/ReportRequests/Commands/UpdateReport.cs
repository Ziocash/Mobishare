using System;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mobishare.Core.Data;
using Mobishare.Core.Models.Vehicles;

namespace Mobishare.Core.Requests.Vehicles.ReportRequests.Commands;

public class UpdateReport : IRequest<Report>
{
    public int Id { get; set; }
    public string Status { get; set; }
}

public class UpdateReportHandler : IRequestHandler<UpdateReport, Report>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<UpdateReportHandler> _logger;

    public UpdateReportHandler(ApplicationDbContext dbContext, ILogger<UpdateReportHandler> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public async Task<Report?> Handle(UpdateReport request, CancellationToken cancellationToken)
    {
        var reportToUpdate = await _dbContext.Reports.FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (reportToUpdate == null)
        {
            _logger.LogWarning("Report with ID {Id} not found", request.Id);
            return null;
        }

        try
        {
            _logger.LogDebug("Executing {method}", nameof(UpdateReportHandler));
            if (reportToUpdate.Status != request.Status)
                reportToUpdate.Status = request.Status;

            reportToUpdate.UserId = reportToUpdate.UserId;
            reportToUpdate.VehicleId = reportToUpdate.VehicleId;
            reportToUpdate.Description = reportToUpdate.Description;
            reportToUpdate.Image = reportToUpdate.Image;

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Report {reportId} updated successfully", reportToUpdate.Id);
            return reportToUpdate;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating report {reportId}", request.Id);
            throw;
        }
    }
}