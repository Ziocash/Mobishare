using System;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mobishare.Core.Data;
using Mobishare.Core.Models.UserRelated;

namespace Mobishare.Core.Requests.Users.HistoryPointRequest.Commands;

public class CreateHistoryPoint : HistoryPoint, IRequest<HistoryPoint> { }

public class CreateHistoryPointHandler : IRequestHandler<CreateHistoryPoint, HistoryPoint>
{
    private readonly IMapper _mapper;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<CreateHistoryPointHandler> _logger;

    public CreateHistoryPointHandler(IMapper mapper, IServiceScopeFactory serviceScopeFactory, ILogger<CreateHistoryPointHandler> logger)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<HistoryPoint> Handle(CreateHistoryPoint request, CancellationToken cancellationToken)
    {
        var newHistoryPoint = _mapper.Map<HistoryPoint>(request);
        try
        {
            _logger.LogDebug("Executing {method}", nameof(CreateHistoryPoint));
            using var scope = _serviceScopeFactory.CreateScope();
            var _dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            _dbContext.HistoryPoints.Entry(newHistoryPoint).State = Microsoft.EntityFrameworkCore.EntityState.Added;
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("History points {historyPoints} created successfully", newHistoryPoint);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating history points");
        }

        return newHistoryPoint;
    }
}