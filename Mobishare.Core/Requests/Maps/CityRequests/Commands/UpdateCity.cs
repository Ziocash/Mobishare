using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mobishare.Core.Data;
using Mobishare.Core.Models.Maps;

namespace Mobishare.Core.Requests.Maps.CityRequests.Commands
{
    public class UpdateCity : City, IRequest<City> { }

    public class UpdateCityHandler : IRequestHandler<UpdateCity, City>
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<UpdateCityHandler> _logger;

        public UpdateCityHandler(ApplicationDbContext dbContext, ILogger<UpdateCityHandler> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Handles the update of a city in the database.
        /// /// </summary>
        /// <param name="request">The request containing the city data to update.</param>
        public async Task<City?> Handle(UpdateCity request, CancellationToken cancellationToken)
        {
            var cityToUpdate = await _dbContext.Cities.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (cityToUpdate == null)
            {
                _logger.LogWarning("City with ID {Id} not found", request.Id);
                return null;
            }

            try
            {
                _logger.LogDebug("Executing {method}", nameof(UpdateCityHandler));

                if (cityToUpdate.Name != request.Name)
                    cityToUpdate.Name = request.Name;
                if (cityToUpdate.PerimeterLocation != request.PerimeterLocation)
                    cityToUpdate.PerimeterLocation = request.PerimeterLocation;
                cityToUpdate.CreatedAt = request.CreatedAt;

                _dbContext.Cities.Update(cityToUpdate);
                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("City {cityId} updated successfully", cityToUpdate.Id);
                return cityToUpdate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating city {cityId}", request.Id);
                throw;
            }
        }
    }
}
