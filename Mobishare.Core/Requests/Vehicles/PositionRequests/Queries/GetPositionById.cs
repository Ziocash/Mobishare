// GetPositionById.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mobishare.Core.Data;
using Mobishare.Core.Models.Vehicles;

namespace Mobishare.Core.Requests.Vehicles.PositionRequests.Queries;

public class GetPositionById : IRequest<Position>
{
    public int PositionId { get; set; }

    public GetPositionById(int positionId)
    {
        PositionId = positionId;
    }
}

public class GetPositionByIdHandler : IRequestHandler<GetPositionById, Position>
{
    private readonly ApplicationDbContext _dbContext;

    public GetPositionByIdHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Position> Handle(GetPositionById request, CancellationToken cancellationToken)
    {
        return await _dbContext.Positions
            .FirstOrDefaultAsync(p => p.Id == request.PositionId, cancellationToken);
    }
}