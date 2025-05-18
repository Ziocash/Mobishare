using System;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Mobishare.Core.Data;
using Mobishare.Core.Models.Maps;

namespace Mobishare.Core.Requests.Maps.CityRequests.Commands;

public class CreateCity : City, IRequest<City> { }

public class CreateCityHandler : IRequestHandler<CreateCity, City>
{
    private readonly IMapper _mapper;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<CreateCityHandler> _logger;

    public CreateCityHandler(IMapper mapper, ApplicationDbContext dbContext, ILogger<CreateCityHandler> logger)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handles the creation of a new city.
    /// This method is responsible for adding a new city to the database.
    /// It uses the provided request object to extract the necessary information for creating the city.
    /// The method is asynchronous and returns a Task of type City.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<City> Handle(CreateCity request, CancellationToken cancellationToken)
    {
        var newCity = _mapper.Map<City>(request);

        try
        {
            _logger.LogDebug("Executing {method}", nameof(CreateCity));
            _dbContext.Cities.Entry(newCity).State = Microsoft.EntityFrameworkCore.EntityState.Added;
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("City {city} created successfully", newCity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating city");
        }

        return newCity;
    }
}