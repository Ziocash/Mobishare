using System;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mobishare.Core.Data;
using Mobishare.Core.Models.Vehicles;

namespace Mobishare.Core.Requests.Vehicles.ReportRequests;

public class GetReportsByUserId : IRequest<IEnumerable<Report>>
{
    public String UserId { get; }
    public GetReportsByUserId(string userId)
    {
        UserId = userId ?? throw new ArgumentNullException(nameof(userId));
    }
}

public class GetReportsByUserIdHandler : IRequestHandler<GetReportsByUserId, IEnumerable<Report>>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<GetReportsByUserIdHandler> _logger;

    public GetReportsByUserIdHandler(ApplicationDbContext dbContext, ILogger<GetReportsByUserIdHandler> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<Report>?> Handle(GetReportsByUserId request, CancellationToken cancellationToken)
    {
        return await _dbContext.ReportAssignments
            .Where(ra => ra.UserId == request.UserId)
            .Include(ra => ra.Report)
                .ThenInclude(r => r.Vehicle)
                    .ThenInclude(v => v.Positions)
            .Select(ra => ra.Report)
            .ToListAsync(cancellationToken);
    }
}