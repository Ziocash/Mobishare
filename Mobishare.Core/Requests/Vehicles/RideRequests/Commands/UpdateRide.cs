using System;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Mobishare.Core.Data;
using Mobishare.Core.Models.Vehicles;

namespace Mobishare.Core.Requests.Vehicles.RideRequests.Commands;

public class UpdateRide : IRequest<Ride>
{
    public int Id { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public double Price { get; set; }
    public int? PositionStartId { get; set; }
    public int? PositionEndId { get; set; }
    public string UserId { get; set; }
    public int VehicleId { get; set; }
    public string? TripName { get; set; }
}

public class UpdateRideHandler : IRequestHandler<UpdateRide, Ride>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<UpdateRideHandler> _logger;
    
    public UpdateRideHandler(ApplicationDbContext dbContext, ILogger<UpdateRideHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    
    public async Task<Ride> Handle(UpdateRide request, CancellationToken cancellationToken)
    {
        var ride = await _dbContext.Rides.FindAsync(request.Id);
        if (ride == null) return null;
        
        ride.EndDateTime = request.EndDateTime;
        ride.PositionEndId = request.PositionEndId;
        ride.Price = request.Price;
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        return ride;
    }
}