using System;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mobishare.Core.Data;
using Mobishare.Core.Models.Vehicles;

namespace Mobishare.Core.Requests.Vehicles.PositionRequests.Commands;

public class CreatePosition : Position, IRequest<Position> { }

public class CreatePositionHandler : IRequestHandler<CreatePosition, Position>
{
    private readonly IMapper _mapper;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<CreatePositionHandler> _logger;

    public CreatePositionHandler(IMapper mapper, IServiceScopeFactory serviceScopeFactory, ILogger<CreatePositionHandler> logger)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handles the creation of a new position.
    /// This method is responsible for adding a new position to the database.
    /// It uses the provided request object to extract the necessary information for creating the position.
    /// The method is asynchronous and returns a Task of type Position.
    /// </summary>
    public async Task<Position> Handle(CreatePosition request, CancellationToken cancellationToken)
    {
        var newPosition = _mapper.Map<Position>(request);

        try
        {
            _logger.LogDebug("Executing {method}", nameof(CreatePosition));
            using var scope = _serviceScopeFactory.CreateScope();
            var _dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            _dbContext.Positions.Entry(newPosition).State = Microsoft.EntityFrameworkCore.EntityState.Added;
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Position {position} created successfully", newPosition);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating position");
        }

        return newPosition;
    }
}
