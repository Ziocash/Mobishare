using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mobishare.Core.Data;
using Mobishare.Core.Models.Maps;

namespace Mobishare.Core.Requests.Maps.CityRequests.Commands;

public class DeleteCity : City, IRequest<City>{}

public class DeleteCityHandler : IRequestHandler<DeleteCity, City>
{
    private readonly IMapper _mapper;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<DeleteCityHandler> _logger;

    public DeleteCityHandler(IMapper mapper, ApplicationDbContext dbContext, ILogger<DeleteCityHandler> logger)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<City> Handle(DeleteCity request, CancellationToken cancellationToken)
    {
        var cityId = _mapper.Map<City>(request);
        var cityToDelete = _dbContext.Cities.Where(x => x.Id == cityId.Id).First();

        if (cityToDelete == null)
        {
            _logger.LogWarning("City with ID {CityId} not found", cityId.Id);
        } else
        {
            try
            {
                _logger.LogDebug("Executing {method}", nameof(DeleteCity));
                _dbContext.Cities.Remove(cityToDelete).State = EntityState.Deleted;
                await _dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("City with ID {CityId} deleted successfully", cityId.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting city with ID {CityId}", cityId.Id);
            }
        }

        return cityId;
    }
}

