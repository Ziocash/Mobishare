namespace Mobishare.App.Services
{
    public interface IGoogleGeocodingService
    {
        /// <summary>
        /// Retrieves a formatted address string corresponding to the provided geographic coordinates.
        /// </summary>
        /// <param name="latitude">The latitude of the geographic location.</param>
        /// <param name="longitude">The longitude of the geographic location.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation.
        /// The result is a <see cref="string"/> containing the formatted address, or <c>null</c> if not found or an error occurs.
        /// </returns>
        Task<string?> GetAddressFromCoordinatesAsync(double latitude, double longitude);
    }
}
