using System;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mobishare.Core.Data;
using Mobishare.Core.Models.Maps;

namespace Mobishare.Core.Requests.Maps.CityRequests.Queries;

public class GetAllCities : IRequest<List<City>> { }

public class GetAllCitiesHandler : IRequestHandler<GetAllCities, List<City>>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<GetAllCitiesHandler> _logger;

    public GetAllCitiesHandler(ApplicationDbContext dbContext, ILogger<GetAllCitiesHandler> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<City>> Handle(GetAllCities request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Executing {method}", nameof(GetAllCities));
            var cities = await _dbContext.Cities.ToListAsync(cancellationToken);
            return cities;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cities");
            return new List<City>();
        }
    }
}