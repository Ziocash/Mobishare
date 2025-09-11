using System;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mobishare.Core.Data;
using Mobishare.Core.Models.Vehicles;

namespace Mobishare.Core.Requests.Vehicles.RepairRequests.Commands;

public class CreateRepair : Repair, IRequest<Repair> { }

public class CreateRepairHandler : IRequestHandler<CreateRepair, Repair>
{
    private readonly IMapper _mapper;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<CreateRepairHandler> _logger;

    public CreateRepairHandler(IMapper mapper, IServiceScopeFactory serviceScopeFactory, ILogger<CreateRepairHandler> logger)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gestisce la creazione di un nuovo record Repair.
    /// </summary>
    public async Task<Repair> Handle(CreateRepair request, CancellationToken cancellationToken)
    {
        var newRepair = _mapper.Map<Repair>(request);

        try
        {
            _logger.LogDebug("Executing {method} for ReportId {ReportId}", nameof(CreateRepair), newRepair.ReportId);
            using var scope = _serviceScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Repairs.Entry(newRepair).State = Microsoft.EntityFrameworkCore.EntityState.Added;
            await dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Repair for ReportId {ReportId} created successfully with Id {RepairId}", newRepair.ReportId, newRepair.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating repair for ReportId {ReportId}", newRepair.ReportId);
        }

        return newRepair;
    }
}