using System;
using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mobishare.Core.Data;
using Mobishare.Core.Models.UserRelated;
using Mobishare.Core.Models.Vehicles;
using Mobishare.Core.Security;

namespace Mobishare.Core.Requests.Users.TechnicianRequest.Queries;

public class TechnicianReportService : IRequest<List<TechnicianReports>> { }

public class TechnicianReportServiceHandler : IRequestHandler<TechnicianReportService, List<TechnicianReports>>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<TechnicianReportServiceHandler> _logger;

    public TechnicianReportServiceHandler(ApplicationDbContext dbContext, ILogger<TechnicianReportServiceHandler> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<TechnicianReports>> Handle(TechnicianReportService request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Executing {method}", nameof(TechnicianReportService));
            var techniciansId = await _dbContext.UserClaims
                            .Where(u =>
                                u.ClaimType == ClaimNames.Role.ToString() &&
                                u.ClaimValue == UserRole.Technician.ToString()
                            ).Select(c => c.UserId)
                            .Distinct()
                            .ToListAsync();

            if (!techniciansId.Any())
            {
                _logger.LogDebug("No technicians found with the specified claim.");
                return new List<TechnicianReports>();
            }

            var technicianStats = await _dbContext.Users
                .Where(user => techniciansId.Contains(user.Id))
                .Select(user => new TechnicianReports 
                {
                    TechnicianId = user.Id,
                    
                    AssignedReports = _dbContext.ReportAssignments
                                        .Count(a => a.UserId == user.Id),
                                        
                    LastClosedReports = _dbContext.ReportAssignments
                        .Where(a => a.UserId == user.Id && a.Report.Status == "Chiuso")
                        .OrderByDescending(a => a.Report.CreatedAt)
                        .Take(5)
                        .Select(a => new ReportSolution
                        {
                            IssueDescription = a.Report.Description,
                            IssueSolution = a.Report.Repairs.FirstOrDefault().Description 
                        })
                        .ToList()
                })
                .ToListAsync(cancellationToken);

            return technicianStats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cities");
            return new List<TechnicianReports>();
        }
    }
}